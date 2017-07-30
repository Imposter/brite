using System;
using System.Threading.Tasks;
using Brite.Micro.Hardware;
using Brite.Micro.Hardware.Memory;
using Brite.Micro.Protocols;
using Brite.Micro.Protocols.STK500v1;
using Brite.Micro.Protocols.STK500v1.Messages;
using Brite.Utility.IO;

namespace Brite.Micro.BootloaderProgrammers
{
    public class OptibootBootloaderProgrammer<TSerialConnection> : ArduinoBootloaderProgrammer<TSerialConnection> where TSerialConnection : ISerialConnection, new()
    {
        private static readonly Log log = Logger.GetLog<OptibootBootloaderProgrammer<TSerialConnection>>();

        public OptibootBootloaderProgrammer(SerialConfig config, IMicrocontroller mcu)
            : base(config, mcu)
        {
        }

        public override async Task Open()
        {
            await base.Open();

            // The Uno (and Nano R3) will have auto-reset because DTR is true when opening the serial connection, 
            // so we just wait a small amount of time for it to come back.
            await Task.Delay(250);
        }

        protected override async Task Reset()
        {
            await ToggleDtrRts(250, 50);
        }

        public override async Task EstablishSync()
        {
            int i;
            for (i = 0; i < MaxSyncRetries; i++)
            {
                await Send(new GetSyncRequest());
                var result = await Receive<GetSyncResponse>();
                if (result == null) continue;
                if (result.IsInSync) break;
            }

            if (i == MaxSyncRetries)
                log.ThrowError("Unable to establish sync after {0} retries.", MaxSyncRetries);

            var nextByte = await ReceiveNext();

            if (nextByte != Constants.RespStkOk)
                log.ThrowError("Unable to establish sync.");
        }

        protected async Task SendWithSyncRetry(IRequest request)
        {
            byte nextByte;
            while (true)
            {
                await Send(request);
                nextByte = (byte)await ReceiveNext();
                if (nextByte == Constants.RespStkNoSync)
                {
                    await EstablishSync();
                    continue;
                }
                break;
            }
            if (nextByte != Constants.RespStkInSync)
                log.ThrowError("Unable to acquire sync in SendWithSyncRetry for request of type {0}!", request.GetType());
        }

        public override async Task CheckDeviceSignature()
        {
            log.Debug("Expecting to find '{0}'...", MCU.DeviceSignature);
            await SendWithSyncRetry(new ReadSignatureRequest());
            var response = await Receive<ReadSignatureResponse>(4);
            if (response == null || !response.IsCorrectResponse)
                log.ThrowError(
                    "Unable to check device signature!");

            var signature = response.Signature;
            if (BitConverter.ToString(signature) != MCU.DeviceSignature)
                log.ThrowError("Unexpected device signature - found '{0}'- expected '{1}'.", BitConverter.ToString(signature), MCU.DeviceSignature);
        }

        public override async Task InitializeDevice()
        {
            var majorVersion = GetParameterValue(Constants.ParmStkSwMajor);
            var minorVersion = GetParameterValue(Constants.ParmStkSwMinor);
            log.Info("Retrieved software version: {0}.{1}", majorVersion, minorVersion);

            log.Info("Setting device programming parameters...");
            await SendWithSyncRetry(new SetDeviceProgrammingParametersRequest(MCU));
            var nextByte = await ReceiveNext();

            if (nextByte != Constants.RespStkOk)
                log.ThrowError("Unable to set device programming parameters!");
        }

        public override async Task EnableProgrammingMode()
        {
            await SendWithSyncRetry(new EnableProgrammingModeRequest());
            var nextByte = await ReceiveNext();
            if (nextByte == Constants.RespStkOk) return;
            if (nextByte == Constants.RespStkNoDevice || nextByte == Constants.RespStkFailed)
                log.ThrowError("Unable to enable programming mode on the device!");
        }

        public override async Task LeaveProgrammingMode()
        {
            await SendWithSyncRetry(new LeaveProgrammingModeRequest());
            var nextByte = await ReceiveNext();
            if (nextByte == Constants.RespStkOk) return;
            if (nextByte == Constants.RespStkNoDevice || nextByte == Constants.RespStkFailed)
                log.ThrowError("Unable to leave programming mode on the device!");
        }

        private async Task<uint> GetParameterValue(byte param)
        {
            log.Trace("Retrieving parameter '{0}'...", param);
            await SendWithSyncRetry(new GetParameterRequest(param));
            var nextByte = await ReceiveNext();
            var paramValue = (uint)nextByte;
            nextByte = await ReceiveNext();

            if (nextByte == Constants.RespStkFailed)
                log.ThrowError("Retrieving parameter '{0}' failed!", param);

            if (nextByte != Constants.RespStkOk)
                log.ThrowError("General protocol error while retrieving parameter '{0}'.", param);

            return paramValue;
        }

        public override async Task ExecuteWritePage(IMemory memory, int offset, byte[] bytes)
        {
            await SendWithSyncRetry(new ExecuteProgramPageRequest(memory, bytes));
            var nextByte = await ReceiveNext();
            if (nextByte == Constants.RespStkOk) return;
            log.ThrowError("Write at offset {0} failed!", offset);
        }

        public override async Task<byte[]> ExecuteReadPage(IMemory memory)
        {
            var pageSize = memory.PageSize;
            await SendWithSyncRetry(new ExecuteReadPageRequest(memory.Type, pageSize));
            var bytes = await ReceiveNext(pageSize);
            if (bytes == null)
                log.ThrowError("Execute read page failed!");

            var nextByte = await ReceiveNext();
            if (nextByte == Constants.RespStkOk) return bytes;
            log.ThrowError("Execute read page failed!");
            return null;
        }

        public override async Task LoadAddress(IMemory memory, int addr)
        {
            log.Trace("Sending load address request: {0}.", addr);
            addr = addr >> 1;
            await SendWithSyncRetry(new LoadAddressRequest(addr));
            var result = await ReceiveNext();
            if (result == Constants.RespStkOk) return;
            log.ThrowError("LoadAddress failed with result {0}!", result);
        }
    }
}
