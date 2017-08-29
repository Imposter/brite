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
        private class ClientInfo
        {
            public ITcpClient InternalClient { get; }
            public string Identifier { get; set; }

            public ClientInfo(ITcpClient client)
            {
                InternalClient = client;
            }
        }

        private class ChannelInfo
        {
            public Animation Animation { get; set; }
            public ClientInfo Client { get; set; }
            public Priority Priority { get; set; }
        }

        private readonly ITcpServer _server;
        private readonly Device _device;
        private readonly List<ClientInfo> _clients;
        private readonly List<BaseAnimation> _animations;
        private readonly Dictionary<Channel, ChannelInfo> _channels;

        public BriteServer(ITcpServer server, Device device)
        {
            _server = server;
            _device = device;

            _clients = new List<ClientInfo>();
            _animations = new List<BaseAnimation>();
            _channels = new Dictionary<Channel, ChannelInfo>();

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

            lock (_channels)
            {
                foreach (var deviceChannel in _device.Channels)
                    _channels.Add(deviceChannel, new ChannelInfo());
            }

            await _server.StartAsync();
        }

        public async Task StopAsync()
        {
            await _server.StopAsync();
            lock (_clients) _clients.Clear();
            lock (_channels) _channels.Clear();
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
            lock (_clients) _clients.Add(new ClientInfo(e.Client));
        }

        private void ServerOnClientDisconnected(object sender, TcpConnectionEventArgs e)
        {
            lock (_clients) _clients.RemoveAll(c => c.InternalClient == e.Client);
        }

        private async void ServerOnOnDataReceived(object sender, TcpReceivedEventArgs e)
        {
            ClientInfo client;
            lock (_clients) client = _clients.First(c => c.InternalClient == e.Client);

            var inputStream = new BinaryStream(new MemoryStream(e.Buffer));
            var outputStream = new BinaryStream(e.Client.GetStream());

            // Read command
            var command = await inputStream.ReadUInt8Async();
            if (command >= (byte)Command.Max)
                return;

            // Write response command
            await outputStream.WriteUInt8Async(command);

            if (client.Identifier == string.Empty && command != (byte)Command.SetId)
            {
                await outputStream.WriteUInt8Async((byte)Result.Error);
                return;
            }

            if (command == (byte)Command.SetId)
            {
                var identifierLength = await inputStream.ReadInt32Async();
                client.Identifier = await inputStream.ReadStringAsync(identifierLength);

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.RequestChannel)
            {
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > _device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.Error);
                    return;
                }

                var priority = await inputStream.ReadUInt8Async();
                if (priority > (byte)Priority.VeryHigh)
                {
                    await outputStream.WriteUInt8Async((byte)Result.Error);
                    return;
                }

                var channel = _device.Channels[channelIndex];
                var channelInfo = _channels[channel];
                if (priority > (byte)channelInfo.Priority)
                {
                    channelInfo.Client = client;
                    channelInfo.Priority = (Priority)priority;

                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                }
                else
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                }
            }
            else if (command == (byte)Command.ReleaseChannel)
            {
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > _device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.Error);
                    return;
                }

                var channel = _device.Channels[channelIndex];
                var channelInfo = _channels[channel];
                if (channelInfo.Client == client)
                {
                    channelInfo.Client = null;
                    channelInfo.Priority = Priority.Normal;

                    await outputStream.WriteUInt8Async((byte)Result.Ok);
                }
                else
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                }
            }
            else if (command == (byte)Command.DeviceGetVersion)
            {
                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt32Async(_device.FirmwareVersion);
            }
            else if (command == (byte)Command.DeviceGetId)
            {
                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt32Async(_device.Id);
            }
            else if (command == (byte)Command.DeviceGetParameters)
            {
                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt8Async(_device.ChannelCount);
                await outputStream.WriteUInt16Async(_device.ChannelMaxSize);
                await outputStream.WriteUInt8Async(_device.ChannelMaxBrightness);
                await outputStream.WriteUInt8Async(_device.AnimationMaxColors);
                await outputStream.WriteFloatAsync(_device.AnimationMinSpeed);
                await outputStream.WriteFloatAsync(_device.AnimationMaxSpeed);
            }
            else if (command == (byte)Command.DeviceGetAnimations)
            {
                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt8Async((byte)_animations.Count);
                foreach (var animation in _animations)
                    await outputStream.WriteUInt32Async(animation.GetId());
            }
            else if (command == (byte)Command.DeviceSynchronize)
            {
                await _device.SynchonizeAsync();

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelBrightness)
            {
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > _device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.Error);
                    return;
                }

                var brightness = await inputStream.ReadUInt8Async();
                var channel = _device.Channels[channelIndex];
                var channelInfo = _channels[channel];
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                await channel.SetBrightnessAsync(brightness);

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelLedCount)
            {
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > _device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.Error);
                    return;
                }

                var size = await inputStream.ReadUInt8Async();
                var channel = _device.Channels[channelIndex];
                var channelInfo = _channels[channel];
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                await channel.SetSizeAsync(size);

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimation)
            {
                // TODO: Implement.
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationEnabled)
            {
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationSpeed)
            {
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationColorCount)
            {
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationColor)
            {
            }
            else if (command == (byte)Command.DeviceSendChannelAnimationRequest)
            {
            }
        }
    }
}
