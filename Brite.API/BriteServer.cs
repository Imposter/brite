using Brite.Utility.IO;
using Brite.Utility.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brite.API.Animations.Server;
using Brite.Utility;

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
            public BaseAnimation Animation { get; set; }
            public ClientInfo Client { get; set; }
            public Priority Priority { get; set; }
            public Mutex Mutex { get; }

            public ChannelInfo()
            {
                Mutex = new Mutex();
            }
        }

        private class DeviceInfo
        {
            public Dictionary<Channel, ChannelInfo> Channels { get; }

            public DeviceInfo()
            {
                Channels = new Dictionary<Channel, ChannelInfo>();
            }
        }

        private readonly ITcpServer _server;
        private readonly List<ClientInfo> _clients;
        private readonly Dictionary<uint, BaseAnimation> _animations;
        private readonly Dictionary<Device, DeviceInfo> _devices;

        public bool Running => _server.Running;

        public BriteServer(ITcpServer server)
        {
            _server = server;

            _clients = new List<ClientInfo>();
            _animations = new Dictionary<uint, BaseAnimation>();
            _devices = new Dictionary<Device, DeviceInfo>();

            // Add server handlers
            _server.OnClientConnected += ServerOnOnClientConnected;
            _server.OnClientDisconnected += ServerOnClientDisconnected;
            _server.OnDataReceived += ServerOnOnDataReceived;
        }

        public async Task StartAsync()
        {
            if (_devices.Count == 0)
                throw new InvalidOperationException("Devices not found");

            if (_animations.Count == 0)
                throw new InvalidOperationException("Animations not found");

            foreach (var pair in _devices)
                foreach (var channel in pair.Key.Channels)
                    pair.Value.Channels.Add(channel, new ChannelInfo());

            await _server.StartAsync();
        }

        public async Task StopAsync()
        {
            await _server.StopAsync();
            lock (_clients) _clients.Clear();

            foreach (var pair in _devices)
                pair.Value.Channels.Clear();
        }

        public void AddAnimation(BaseAnimation animation)
        {
            _animations.Add(animation.GetId(), animation);
        }

        public void AddAnimations(IEnumerable<BaseAnimation> animations)
        {
            foreach (var animation in animations)
                _animations.Add(animation.GetId(), animation);
        }

        public void AddDevice(Device device)
        {
            _devices.Add(device, new DeviceInfo());
        }

        public void AddDevices(IEnumerable<Device> devices)
        {
            foreach (var device in devices)
                _devices.Add(device, new DeviceInfo());
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

            // TODO: Remove any lock the client has on a channel
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
                await outputStream.WriteUInt8Async((byte)Result.NotIdentified);
                return;
            }

            if (command == (byte)Command.SetId)
            {
                var identifierLength = await inputStream.ReadInt32Async();
                client.Identifier = await inputStream.ReadStringAsync(identifierLength);

                // TODO: Check if client using specified ID already exists

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.GetDevices)
            {
                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt8Async((byte)_devices.Count);
                foreach (var pair in _devices)
                    await outputStream.WriteUInt32Async(pair.Key.Id);
            }
            else if (command == (byte)Command.RequestDeviceChannel)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var priority = await inputStream.ReadUInt8Async();
                if (priority > (byte)Priority.VeryHigh)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidPriority);
                    return;
                }

                // TODO: Wait here indefinitely, until the appropriate permissions are had, and send the client the OK to lock the channel (until the client disconnects)
                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
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
            else if (command == (byte)Command.ReleaseDeviceChannel)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                // TODO: Unlock channel lock mutex
                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
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
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt32Async(device.FirmwareVersion);
            }
            else if (command == (byte)Command.DeviceGetParameters)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt8Async(device.ChannelCount);
                await outputStream.WriteUInt16Async(device.ChannelMaxSize);
                await outputStream.WriteUInt8Async(device.ChannelMaxBrightness);
                await outputStream.WriteUInt8Async(device.AnimationMaxColors);
                await outputStream.WriteFloatAsync(device.AnimationMinSpeed);
                await outputStream.WriteFloatAsync(device.AnimationMaxSpeed);
            }
            else if (command == (byte)Command.DeviceGetAnimations)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var supportedAnimations = (from pair in _animations where device.SupportedAnimations.Contains(pair.Key) select pair.Key).ToArray();
                if (supportedAnimations.Length == 0)
                {
                    await outputStream.WriteUInt8Async((byte)Result.NoSupportedAnimations);
                    return;
                }

                await outputStream.WriteUInt8Async((byte)Result.Ok);
                await outputStream.WriteUInt8Async((byte)supportedAnimations.Length);
                foreach (var anim in supportedAnimations)
                    await outputStream.WriteUInt32Async(anim);
            }
            else if (command == (byte)Command.DeviceSynchronize)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                await device.SynchonizeAsync();

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelBrightness)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                var brightness = await inputStream.ReadUInt8Async();
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
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                var size = await inputStream.ReadUInt8Async();
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
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                var animId = await inputStream.ReadUInt32Async();
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                if (!_animations.ContainsKey(animId))
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidAnimationId);
                    return;
                }

                var animation = _animations[animId];
                channelInfo.Animation = animation;
                await channel.SetAnimationAsync((Animation)Activator.CreateInstance(animation.GetAnimation()));

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationEnabled)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                var enabled = await inputStream.ReadBooleanAsync();
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetEnabledAsync(enabled);

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationSpeed)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                var speed = await inputStream.ReadFloatAsync();
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetSpeedAsync(speed);

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationColorCount)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                var count = await inputStream.ReadUInt8Async();
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetColorCountAsync(count);

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationColor)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                var index = await inputStream.ReadUInt8Async();
                var r = await inputStream.ReadUInt8Async();
                var g = await inputStream.ReadUInt8Async();
                var b = await inputStream.ReadUInt8Async();
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetColorAsync(index, new Color(r, g, b));

                await outputStream.WriteUInt8Async((byte)Result.Ok);
            }
            else if (command == (byte)Command.DeviceSendChannelAnimationRequest)
            {
                var deviceId = await inputStream.ReadUInt32Async();
                var device = (from pair in _devices where pair.Key.Id == deviceId select pair.Key).First();
                if (device == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                var channelIndex = await inputStream.ReadUInt8Async();
                if (channelIndex > device.Channels.Length)
                {
                    await outputStream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await outputStream.WriteUInt8Async((byte)Result.AnimationNotSet);
                    return;
                }

                await outputStream.WriteUInt8Async((byte)Result.Ok);

                await channelInfo.Animation.HandleRequestAsync(channel, inputStream, outputStream);
            }
        }
    }
}
