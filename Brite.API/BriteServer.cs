using Brite.Utility.IO;
using Brite.Utility.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brite.API.Animations.Server;

namespace Brite.API
{
    public class BriteServer : IDisposable
    {
        private class Client
        {
            public ITcpClient InternalClient { get; }
            public string Identifier { get; set; }
            public Dictionary<Channel, Priority> Channels { get; }

            public Client(ITcpClient client)
            {
                InternalClient = client;
                Channels = new Dictionary<Channel, Priority>();
            }
        }

        private readonly ITcpServer _server;
        private readonly Device _device;
        private readonly List<Client> _clients;
        private readonly List<BaseAnimation> _animations;
        private readonly Dictionary<Channel, BaseAnimation> _channelAnimations;

        public BriteServer(ITcpServer server, Device device)
        {
            _server = server;
            _device = device;

            _clients = new List<Client>();
            _animations = new List<BaseAnimation>();
            _channelAnimations = new Dictionary<Channel, BaseAnimation>();

            // Add server handlers
            _server.OnClientConnected += ServerOnOnClientConnected;
            _server.OnClientDisconnected += ServerOnClientDisconnected;
            _server.OnDataReceived += ServerOnOnDataReceived;
        }

        public async Task StartAsync()
        {
            if (!_device.IsOpen)
                throw new InvalidOperationException("Device not open");

            if (_animations.Count == 0)
                throw new InvalidOperationException("Animations not found");

            await _server.StartAsync();
        }

        public async Task StopAsync()
        {
            await _server.StopAsync();
            lock (_clients) _clients.Clear();
            lock (_channelAnimations) _channelAnimations.Clear();
        }

        public void AddAnimation(BaseAnimation animation)
        {
            _animations.Add(animation);
        }

        public void AddAnimations(IEnumerable<BaseAnimation> animations)
        {
            _animations.AddRange(animations);
        }

        public void Dispose()
        {
            _server.OnClientConnected -= ServerOnOnClientConnected;
            _server.OnClientDisconnected -= ServerOnClientDisconnected;
            _server.OnDataReceived -= ServerOnOnDataReceived;
        }

        private void ServerOnOnClientConnected(object sender, TcpConnectionEventArgs e)
        {
            lock (_clients) _clients.Add(new Client(e.Client));
        }

        private void ServerOnClientDisconnected(object sender, TcpConnectionEventArgs e)
        {
            lock (_clients) _clients.RemoveAll(c => c.InternalClient == e.Client);
        }

        private async void ServerOnOnDataReceived(object sender, TcpReceivedEventArgs e)
        {
            Client client;
            lock (_clients) client = _clients.First(c => c.InternalClient == e.Client);

            var inputStream = new BinaryStream(new MemoryStream(e.Buffer));
            var outputStream = new BinaryStream(e.Client.GetStream());

            // Read command
            var command = await inputStream.ReadUInt8Async();
            if (command >= (byte)Command.Max)
                return;

            // Write response command
            await outputStream.WriteUInt8Async(command);

            switch ((Command)command)
            {
                case Command.SetId:
                    var identifierLength = await inputStream.ReadInt32Async();
                    client.Identifier = await inputStream.ReadStringAsync(identifierLength);

                    // Write response
                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                    break;

                case Command.RequestChannel:
                    // TODO: Implement
                    break;

                case Command.ReleaseChannel:
                    // TODO: Implement
                    break;

                case Command.DeviceGetVersion:
                    // Write response
                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                    await outputStream.WriteUInt32Async(_device.FirmwareVersion);
                    break;

                case Command.DeviceGetId:
                    // Write response
                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                    await outputStream.WriteUInt32Async(_device.Id);
                    break;

                case Command.DeviceGetParameters:
                    // Write response
                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                    await outputStream.WriteUInt8Async(_device.ChannelCount);
                    await outputStream.WriteUInt16Async(_device.ChannelMaxSize);
                    await outputStream.WriteUInt8Async(_device.ChannelMaxBrightness);
                    await outputStream.WriteUInt8Async(_device.AnimationMaxColors);
                    await outputStream.WriteFloatAsync(_device.AnimationMinSpeed);
                    await outputStream.WriteFloatAsync(_device.AnimationMaxSpeed);
                    break;

                case Command.DeviceGetAnimations:
                    // Write response
                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                    await outputStream.WriteUInt8Async((byte)_animations.Count);
                    foreach (var animation in _animations)
                        await outputStream.WriteUInt32Async(animation.GetId());
                    break;

                case Command.DeviceSynchronize:
                    await _device.SynchonizeAsync();

                    // Write response
                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                    break;

                case Command.DeviceSetChannelBrightness:

                    break;

                case Command.DeviceSetChannelLedCount:

                    break;

                case Command.DeviceSetChannelAnimation:

                    break;

                case Command.DeviceSetChannelAnimationEnabled:

                    break;

                case Command.DeviceSetChannelAnimationSpeed:

                    break;

                case Command.DeviceSetChannelAnimationColorCount:

                    break;

                case Command.DeviceSetChannelAnimationColor:

                    break;

                case Command.DeviceSendChannelAnimationRequest:

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
