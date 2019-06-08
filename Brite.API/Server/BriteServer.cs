/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Brite.API.Animations.Server;
using Brite.Utility.IO;
using Brite.Utility.Network;

namespace Brite.API.Server
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
                Identifier = string.Empty;
            }
        }

        private class ChannelInfo
        {
            public BaseAnimation Animation { get; set; }
            public ClientInfo Client { get; set; }
            public PriorityMutex<Priority> Mutex { get; }

            public ChannelInfo()
            {
                Mutex = new PriorityMutex<Priority>();
            }
        }

        private class DeviceInfo
        {
            public uint BaudRate { get; set; }
            public Dictionary<Channel, ChannelInfo> Channels { get; }
            public ClientInfo Client { get; set; }
            public PriorityMutex<Priority> Mutex { get; }

            public DeviceInfo(uint baudRate)
            {
                BaudRate = baudRate;
                Channels = new Dictionary<Channel, ChannelInfo>();
                Mutex = new PriorityMutex<Priority>();
            }
        }

        private const uint ProtocolHeader = 0xB17EFEED;

        private static readonly Log Log = Logger.GetLog<BriteServer>();

        private readonly ITcpServer _server;
        private readonly int _deviceTimeout;
        private readonly int _deviceRetries;
        private readonly int _deviceConnectionRetries;
        private readonly List<ClientInfo> _clients;
        private readonly Dictionary<uint, BaseAnimation> _animations;
        private readonly Dictionary<Device, DeviceInfo> _devices;

        public bool Running => _server.Running;

        public BriteServer(ITcpServer server, int deviceTimeout, int deviceRetries, int deviceConnectionRetries)
        {
            _server = server;
            _deviceTimeout = deviceTimeout;
            _deviceRetries = deviceRetries;
            _deviceConnectionRetries = deviceConnectionRetries;

            _clients = new List<ClientInfo>();
            _animations = new Dictionary<uint, BaseAnimation>();
            _devices = new Dictionary<Device, DeviceInfo>();

            // Configure server to automatically read data
            _server.AutoReceive = true;

            // Add server handlers
            _server.OnClientConnected += ServerOnOnClientConnected;
            _server.OnClientDisconnected += ServerOnClientDisconnected;
            _server.OnDataReceived += ServerOnDataReceived;
        }

        public async Task StartAsync()
        {
            if (_devices.Count == 0)
                throw new InvalidOperationException("Devices not found");

            if (_animations.Count == 0)
                throw new InvalidOperationException("Animations not found");

            // Connect to devices
            foreach (var pair in _devices)
            {
                for (var i = 0; i < _deviceConnectionRetries; i++)
                {
                    try
                    {
                        // Open connection to device
                        await pair.Key.OpenAsync(pair.Value.BaudRate, _deviceTimeout, _deviceRetries);
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (i == _deviceConnectionRetries - 1)
                        {
                            await Log.ErrorAsync($"Failed to connect to {pair.Key.Info.PortName} using rate {pair.Value.BaudRate}, ignoring device...");
                            await Log.ErrorAsync(ex.ToString());
                        }
                    }
                }
            }

            foreach (var pair in _devices)
                foreach (var channel in pair.Key.Channels)
                    pair.Value.Channels.Add(channel, new ChannelInfo());

            await _server.StartAsync();

            await Log.InfoAsync($"Listening on port {_server.ListenEndPoint.Port}");
        }

        public async Task StopAsync()
        {
            await _server.StopAsync();
            lock (_clients) _clients.Clear();

            foreach (var pair in _devices)
                pair.Value.Channels.Clear();

            // Disconnect from devices
            foreach (var pair in _devices)
            {
                if (pair.Key.IsOpen)
                    await pair.Key.CloseAsync();

                // Unlock device if needed
                if (pair.Value.Client != null)
                {
                    pair.Value.Mutex.Unlock(pair.Value.Client);
                    pair.Value.Client = null;
                }
            }

            await Log.InfoAsync("Stopped listening");
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

        public void AddDevice(Device device, uint baudRate)
        {
            _devices.Add(device, new DeviceInfo(baudRate));
        }

        public void Dispose()
        {
            _server.OnClientConnected -= ServerOnOnClientConnected;
            _server.OnClientDisconnected -= ServerOnClientDisconnected;
            _server.OnDataReceived -= ServerOnDataReceived;
        }

        private async Task SendResponse(Message request, Result result, BinaryStream outputStream = null)
        {
            // Write header
            var stream = request.InternalStream;
            await stream.WriteUInt32Async(ProtocolHeader);
            await stream.WriteUInt8Async((byte)request.Command);
            await stream.WriteInt32Async(request.Id);
            await stream.WriteUInt8Async((byte)result);

            if (outputStream != null)
            {
                var outData = ((MemoryStream)outputStream.Stream).ToArray();
                await stream.WriteInt32Async(outData.Length);
                await stream.WriteBlobAsync(outData);
            }
            else
            {
                await stream.WriteInt32Async(0);
            }
        }

        private async void ServerOnOnClientConnected(object sender, TcpConnectionEventArgs e)
        {
            await Log.InfoAsync($"Client connected from {e.Source}");

            var client = new ClientInfo(e.Client);
            lock (_clients) _clients.Add(client);

            // Set infinite timeout
            e.Client.Timeout = -1;
        }

        private async void ServerOnClientDisconnected(object sender, TcpConnectionEventArgs e)
        {
            await Log.InfoAsync($"Client disconnected from {e.Source}");

            // Remove locks on devices and channels
            foreach (var devicePair in _devices)
            {
                foreach (var channelPair in devicePair.Value.Channels)
                {
                    var channelInfo = channelPair.Value;
                    if (channelInfo.Client != null && channelInfo.Client.InternalClient == e.Client)
                    {
                        channelInfo.Mutex.RemoveLockRequest(channelInfo.Client);
                        channelInfo.Client = null;

                        await Log.InfoAsync($"Releasing channel from disconnected client: {e.Source}");
                    }
                }

                var deviceInfo = devicePair.Value;
                if (deviceInfo.Client != null && deviceInfo.Client.InternalClient == e.Client)
                {
                    deviceInfo.Mutex.RemoveLockRequest(deviceInfo.Client);
                    deviceInfo.Client = null;

                    await Log.InfoAsync($"Releasing device from disconnected client: {e.Source}");
                }
            }

            // Remove from client list
            lock (_clients) _clients.RemoveAll(c => c.InternalClient == e.Client);
        }

        private async void ServerOnDataReceived(object sender, TcpReceivedEventArgs e)
        {
            // Ensure the data contains a header
            if (e.Length < 13) return;

            // Get client
            ClientInfo client;
            lock (_clients) client = _clients.Find(c => c.InternalClient == e.Client);

            // Create streams
            var internalStream = new BinaryStream(e.Client.GetStream());
            var stream = new BinaryStream(new MemoryStream(e.Buffer, 0, e.Length));

            // Read header
            var protocolHeader = await stream.ReadUInt32Async();
            if (protocolHeader != ProtocolHeader)
                return; // TODO: Disconnect client

            var command = await stream.ReadUInt8Async();
            if (command >= (byte)Command.Max)
                return;

            var id = await stream.ReadInt32Async();
            var inDataLength = await stream.ReadInt32Async();
            var inData = await stream.ReadBlobAsync(inDataLength);
            var dataStream = new BinaryStream(new MemoryStream(inData));

            await Log.TraceAsync($"Received {(Command)command} request from {e.Source}");

            // Create message
            var request = new Message(internalStream, (Command)command, id, dataStream);

            // Handle message
            if (string.IsNullOrWhiteSpace(client.Identifier) && command != (byte)Command.SetId)
            {
                await SendResponse(request, Result.NotIdentified);
                return;
            }

            if (command == (byte)Command.SetId)
            {
                var identifierLength = await request.Stream.ReadInt32Async();
                var identifier = await request.Stream.ReadStringAsync(identifierLength);

                if (string.IsNullOrWhiteSpace(identifier))
                {
                    await SendResponse(request, Result.InvalidId);
                    return;
                }

                // NOTE: Using LINQ here somehow causes a deadlock?
                ClientInfo existingClient = null;
                lock (_clients) // TODO: Lock clients in another way
                {
                    foreach (var clientInfo in _clients)
                    {
                        if (clientInfo.Identifier == identifier)
                        {
                            existingClient = clientInfo;
                            break;
                        }
                    }
                }

                if (existingClient != null)
                {
                    await SendResponse(request, Result.IdInUse);
                    return;
                }

                client.Identifier = identifier;

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.GetDevices)
            {
                var outputStream = new BinaryStream(new MemoryStream());
                await outputStream.WriteUInt8Async((byte)_devices.Count);
                foreach (var pair in _devices)
                    await outputStream.WriteUInt32Async(pair.Key.Id);

                await SendResponse(request, Result.Ok, outputStream);
            }
            else if (command == (byte)Command.RequestDevice)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var priority = await request.Stream.ReadUInt8Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                if (priority > (byte)Priority.VeryHigh)
                {
                    await SendResponse(request, Result.InvalidPriority);
                    return;
                }

                var deviceInfo = _devices[device];
                if (deviceInfo.Client != null)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                // Unlock channels if needed
                foreach (var pair in deviceInfo.Channels)
                {
                    if (pair.Value.Client != null)
                        pair.Value.Mutex.Unlock(pair.Value.Client);
                }


                // Lock device
                await deviceInfo.Mutex.LockAsync(client, (Priority)priority);

                deviceInfo.Client = client;

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.ReleaseDevice)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                if (deviceInfo.Client == client)
                {
                    // Unlock device lock mutex to allow other clients to lock the device
                    deviceInfo.Mutex.Unlock(deviceInfo.Client);

                    deviceInfo.Client = null;

                    await SendResponse(request, Result.Ok);
                }
                else
                {
                    await SendResponse(request, Result.AccessDenied);
                }
            }
            else if (command == (byte)Command.OpenDevice)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                if (deviceInfo.Client == client)
                {
                    var errorOccurred = false;
                    for (var i = 0; i < _deviceConnectionRetries; i++)
                    {
                        try
                        {
                            // Open connection to device
                            await device.OpenAsync(deviceInfo.BaudRate, _deviceTimeout, _deviceRetries);
                            break;
                        }
                        catch
                        {
                            if (i == _deviceConnectionRetries - 1)
                            {
                                await SendResponse(request, Result.Error);
                                errorOccurred = true;
                                break;
                            }
                        }
                    }

                    if (!errorOccurred)
                        await SendResponse(request, Result.Ok);
                }
                else
                {
                    await SendResponse(request, Result.AccessDenied);
                }
            }
            else if (command == (byte)Command.CloseDevice)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];
                if (deviceInfo.Client == client)
                {
                    // Close device
                    await device.CloseAsync();

                    await SendResponse(request, Result.Ok);
                }
                else
                {
                    await SendResponse(request, Result.AccessDenied);
                }
            }
            else if (command == (byte)Command.RequestDeviceChannel)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var priority = await request.Stream.ReadUInt8Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                if (priority > (byte)Priority.VeryHigh)
                {
                    await SendResponse(request, Result.InvalidPriority);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                // Wait here indefinitely, until the appropriate permissions are had
                // and send the client the OK to lock the channel (until the client disconnects)
                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                channelInfo.Client = client;

                await channelInfo.Mutex.LockAsync(channelInfo.Client, (Priority)priority);

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.ReleaseDeviceChannel)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client == client)
                {
                    // Unlock channel lock mutex to allow other clients to use the channel
                    channelInfo.Mutex.Unlock(channelInfo.Client);

                    channelInfo.Client = null;

                    await SendResponse(request, Result.Ok);
                }
                else
                {
                    await SendResponse(request, Result.AccessDenied);
                }
            }
            else if (command == (byte)Command.DeviceGetVersion)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var outputStream = new BinaryStream(new MemoryStream());
                await outputStream.WriteUInt32Async(device.FirmwareVersion);

                await SendResponse(request, Result.Ok, outputStream);
            }
            else if (command == (byte)Command.DeviceGetParameters)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var outputStream = new BinaryStream(new MemoryStream());
                await outputStream.WriteUInt8Async(device.ChannelCount);
                await outputStream.WriteUInt16Async(device.ChannelMaxSize);
                await outputStream.WriteUInt8Async(device.ChannelMaxBrightness);
                await outputStream.WriteUInt8Async(device.AnimationMaxColors);
                await outputStream.WriteFloatAsync(device.AnimationMinSpeed);
                await outputStream.WriteFloatAsync(device.AnimationMaxSpeed);

                await SendResponse(request, Result.Ok, outputStream);
            }
            else if (command == (byte)Command.DeviceGetAnimations)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var supportedAnimations = (_animations
                    .Where(pair => device.SupportedAnimations.Contains(pair.Key))
                    .Select(pair => pair.Key)).ToArray();
                if (supportedAnimations.Length == 0)
                {
                    await SendResponse(request, Result.NoSupportedAnimations);
                    return;
                }

                var outputStream = new BinaryStream(new MemoryStream());
                await outputStream.WriteUInt8Async((byte)supportedAnimations.Length);
                foreach (var anim in supportedAnimations)
                    await outputStream.WriteUInt32Async(anim);

                await SendResponse(request, Result.Ok, outputStream);
            }
            else if (command == (byte)Command.DeviceSynchronize)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                await device.SynchonizeAsync();

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceChannelReset)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                await channel.ResetAsync();

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelBrightness)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var brightness = await request.Stream.ReadUInt8Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                await channel.SetBrightnessAsync(brightness);

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelLedCount)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var size = await request.Stream.ReadUInt16Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                await channel.SetSizeAsync(size);

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimation)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var animId = await request.Stream.ReadUInt32Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (!_animations.ContainsKey(animId))
                {
                    await SendResponse(request, Result.InvalidAnimationId);
                    return;
                }

                var animation = _animations[animId];
                channelInfo.Animation = animation;
                await channel.SetAnimationAsync(
                    (Animation)Activator.CreateInstance(animation.GetAnimation()));

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationEnabled)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var enabled = await request.Stream.ReadBooleanAsync();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await SendResponse(request, Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetEnabledAsync(enabled);

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationSpeed)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var speed = await request.Stream.ReadFloatAsync();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await SendResponse(request, Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetSpeedAsync(speed);

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationColorCount)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var count = await request.Stream.ReadUInt8Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await SendResponse(request, Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetColorCountAsync(count);

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSetChannelAnimationColor)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();
                var index = await request.Stream.ReadUInt8Async();
                var r = await request.Stream.ReadUInt8Async();
                var g = await request.Stream.ReadUInt8Async();
                var b = await request.Stream.ReadUInt8Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await SendResponse(request, Result.AnimationNotSet);
                    return;
                }

                await channel.Animation.SetColorAsync(index, new Color(r, g, b));

                await SendResponse(request, Result.Ok);
            }
            else if (command == (byte)Command.DeviceSendChannelAnimationRequest)
            {
                var deviceId = await request.Stream.ReadUInt32Async();
                var channelIndex = await request.Stream.ReadUInt8Async();

                var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                if (device == null)
                {
                    await SendResponse(request, Result.InvalidDeviceId);
                    return;
                }

                var deviceInfo = _devices[device];

                // Check if the device is currently locked by another client
                if (deviceInfo.Client != null && deviceInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channelIndex > device.Channels.Length)
                {
                    await SendResponse(request, Result.InvalidChannelIndex);
                    return;
                }

                // Check if device is open
                if (!device.IsOpen)
                {
                    await SendResponse(request, Result.DeviceNotOpen);
                    return;
                }

                var channel = device.Channels[channelIndex];
                var channelInfo = deviceInfo.Channels[channel];
                if (channelInfo.Client != client)
                {
                    await SendResponse(request, Result.AccessDenied);
                    return;
                }

                if (channel.Animation == null)
                {
                    await SendResponse(request, Result.AnimationNotSet);
                    return;
                }

                await channelInfo.Animation.HandleRequestAsync(channel, request.Stream);

                await SendResponse(request, Result.Ok);
            }
        }
    }
}
