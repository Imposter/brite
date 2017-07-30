using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.Micro.BootloaderProgrammers;
using Brite.Micro.Hardware;
using Brite.Micro.Intel;
using Brite.Utility.IO;

namespace Brite.Micro
{
    public class DeviceUploader
    {
        private static readonly Log log = Logger.GetLog<DeviceUploader>();

        // TODO: write this quickly
        public DeviceUploader(ISerialConnection connection)
        {
            // TODO: Use info to create uploader and upload data
        }

        public static async Task Upload(IBootloaderProgrammer programmer, IMicrocontroller microcontroller, byte[] binary)
        {
            try
            {
                await programmer.Open();

                log.Info("Establishing sync...");
                await programmer.EstablishSync();
                log.Info("Sync established.");

                log.Info("Checking device signature...");
                await programmer.CheckDeviceSignature();
                log.Info("Device signature checked.");

                log.Info("Initializing device...");
                await programmer.InitializeDevice();
                log.Info("Device initialized.");

                log.Info("Enabling programming mode on the device...");
                await programmer.EnableProgrammingMode();
                log.Info("Programming mode enabled.");

                log.Info("Programming device...");
                await programmer.ProgramDevice(ReadHexFile(binary, microcontroller.Flash.Size));
                log.Info("Device programmed.");

                log.Info("Leaving programming mode...");
                await programmer.LeaveProgrammingMode();
                log.Info("Left programming mode!");
            }
            finally
            {
                await programmer.Close();
            }

            log.Info("All done, shutting down!");
        }

        private static MemoryBlock ReadHexFile(byte[] hexFileContents, int memorySize)
        {
            try
            {
                var reader = new HexFileReader(hexFileContents, memorySize);
                return reader.Parse();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                throw;
            }
        }
    }
}
