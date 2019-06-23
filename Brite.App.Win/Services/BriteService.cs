using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Brite.API.Client;
using Brite.Utility.Hardware.Serial;
using Brite.Win.Core.Hardware.Serial;
using Brite.Win.Core.Network;

namespace Brite.App.Win.Services
{
    public class BriteService : IBriteService
    {
        private readonly TcpClient _tcpClient;
        private readonly BriteClient _client;


        // TODO: Get port from Config service?
        public BriteService()
        {
            // TODO: Don't keep port a constant
            _client = new BriteClient(_tcpClient = new TcpClient(new IPEndPoint(IPAddress.Any, 6450)), GetType().FullName);
        }

        public async Task<IList<SerialDeviceInfo>> GetHardwareDevicesAsync()
        {
            // Find devices
            var deviceSearcher = new SerialDeviceSearcher();
            return await deviceSearcher.GetDevicesAsync();
        }

        public async Task<IList<BriteDevice>> GetDevicesAsync()
        {
            await EnsureConnectedAsync();
            return _client.Devices;
        }

        public Task UpdateFirmwareAsync(BriteDevice device, string firmwarePath)
        {
            throw new NotImplementedException();
        }

        private async Task EnsureConnectedAsync()
        {
            if (!_tcpClient.Connected)
                await _client.ConnectAsync();
        }

        private async Task EnsureDisconnectedAsync()
        {
            if (_tcpClient.Connected)
                await _client.DisconnectAsync();
        }
    }
}
