/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

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
                Identifier = string.Empty;
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
            public uint BaudRate { get; set; }
            public Dictionary<Channel, ChannelInfo> Channels { get; }
            public ClientInfo Client { get; set; }
            public Priority Priority { get; set; }
            public Mutex Mutex { get; }

            public DeviceInfo(uint baudRate)
            {
                BaudRate = baudRate;
                Channels = new Dictionary<Channel, ChannelInfo>();
                Mutex = new Mutex();
            }
        }

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

            // Add server handlers
            _server.OnClientConnected += ServerOnOnClientConnected;
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
                    pair.Value.Mutex.Unlock();
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
        }

        private async void ServerOnOnClientConnected(object sender, TcpConnectionEventArgs e)
        {
            await Log.InfoAsync($"Client connected from {e.Source}");

            lock (_clients) _clients.Add(new ClientInfo(e.Client));

            // Set infinite timeout
            e.Client.Timeout = -1;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                try
                {
                    while (e.Client.Connected)
                    {
                        ClientInfo client;
                        lock (_clients) client = _clients.First(c => c.InternalClient == e.Client);

                        var stream = new BinaryStream(e.Client.GetStream());

                        // Read command
                        var command = await stream.ReadUInt8Async();
                        if (command >= (byte)Command.Max)
                            continue;

                        await Log.TraceAsync($"Received {(Command)command} request from {e.Source}");

                        // Write response command
                        await stream.WriteUInt8Async(command);

                        if (string.IsNullOrWhiteSpace(client.Identifier) && command != (byte)Command.SetId)
                        {
                            await stream.WriteUInt8Async((byte)Result.NotIdentified);
                            continue;
                        }

                        if (command == (byte)Command.SetId)
                        {
                            var identifierLength = await stream.ReadInt32Async();
                            var identifier = await stream.ReadStringAsync(identifierLength);

                            if (string.IsNullOrWhiteSpace(identifier))
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidId);
                                continue;
                            }

                            // NOTE: Using LINQ here somehow causes a deadlock?
                            ClientInfo existingClient = null;
                            lock (_clients)
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
                                await stream.WriteUInt8Async((byte)Result.IdInUse);
                                continue;
                            }

                            client.Identifier = identifier;

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.GetDevices)
                        {
                            await stream.WriteUInt8Async((byte)Result.Ok);
                            await stream.WriteUInt8Async((byte)_devices.Count);
                            foreach (var pair in _devices)
                                await stream.WriteUInt32Async(pair.Key.Id);
                        }
                        else if (command == (byte)Command.RequestDevice)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var priority = await stream.ReadUInt8Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            if (priority > (byte)Priority.VeryHigh)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidPriority);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (deviceInfo.Client != null)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            // Wait here indefinitely, until the appropriate permissions are had
                            // and send the client the OK to lock the device (until the client disconnects)
                            if (priority <= (byte)deviceInfo.Priority)
                                await deviceInfo.Mutex.LockAsync();

                            deviceInfo.Client = client;
                            deviceInfo.Priority = (Priority)priority;

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.ReleaseDevice)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (deviceInfo.Client == client)
                            {
                                deviceInfo.Client = null;
                                deviceInfo.Priority = Priority.Normal;

                                await stream.WriteUInt8Async((byte)Result.Ok);

                                // Unlock device lock mutex to allow other clients to lock the device
                                deviceInfo.Mutex.Unlock();
                            }
                            else
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                            }
                        }
                        else if (command == (byte)Command.OpenDevice)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (deviceInfo.Client == client)
                            {
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
                                            await stream.WriteUInt8Async((byte)Result.Error);
                                            return;
                                        }
                                    }
                                }

                                await stream.WriteUInt8Async((byte)Result.Ok);
                            }
                            else
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                            }
                        }
                        else if (command == (byte)Command.CloseDevice)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (deviceInfo.Client == client)
                            {
                                // Close device
                                await device.CloseAsync();

                                await stream.WriteUInt8Async((byte)Result.Ok);
                            }
                            else
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                            }
                        }
                        else if (command == (byte)Command.RequestDeviceChannel)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var priority = await stream.ReadUInt8Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            if (priority > (byte)Priority.VeryHigh)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidPriority);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            // Wait here indefinitely, until the appropriate permissions are had
                            // and send the client the OK to lock the channel (until the client disconnects)
                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (priority <= (byte)channelInfo.Priority)
                                await channelInfo.Mutex.LockAsync();

                            channelInfo.Client = client;
                            channelInfo.Priority = (Priority)priority;

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.ReleaseDeviceChannel)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client == client)
                            {
                                channelInfo.Client = null;
                                channelInfo.Priority = Priority.Normal;

                                await stream.WriteUInt8Async((byte)Result.Ok);

                                // Unlock channel lock mutex to allow other clients to use the channel
                                channelInfo.Mutex.Unlock();
                            }
                            else
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                            }
                        }
                        else if (command == (byte)Command.DeviceGetVersion)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            await stream.WriteUInt8Async((byte)Result.Ok);
                            await stream.WriteUInt32Async(device.FirmwareVersion);
                        }
                        else if (command == (byte)Command.DeviceGetParameters)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            await stream.WriteUInt8Async((byte)Result.Ok);
                            await stream.WriteUInt8Async(device.ChannelCount);
                            await stream.WriteUInt16Async(device.ChannelMaxSize);
                            await stream.WriteUInt8Async(device.ChannelMaxBrightness);
                            await stream.WriteUInt8Async(device.AnimationMaxColors);
                            await stream.WriteFloatAsync(device.AnimationMinSpeed);
                            await stream.WriteFloatAsync(device.AnimationMaxSpeed);
                        }
                        else if (command == (byte)Command.DeviceGetAnimations)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var supportedAnimations = (_animations
                                .Where(pair => device.SupportedAnimations.Contains(pair.Key))
                                .Select(pair => pair.Key)).ToArray();
                            if (supportedAnimations.Length == 0)
                            {
                                await stream.WriteUInt8Async((byte)Result.NoSupportedAnimations);
                                continue;
                            }

                            await stream.WriteUInt8Async((byte)Result.Ok);
                            await stream.WriteUInt8Async((byte)supportedAnimations.Length);
                            foreach (var anim in supportedAnimations)
                                await stream.WriteUInt32Async(anim);
                        }
                        else if (command == (byte)Command.DeviceSynchronize)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            await device.SynchonizeAsync();

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceChannelReset)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            await channel.ResetAsync();

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSetChannelBrightness)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var brightness = await stream.ReadUInt8Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            await channel.SetBrightnessAsync(brightness);

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSetChannelLedCount)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var size = await stream.ReadUInt16Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            await channel.SetSizeAsync(size);

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSetChannelAnimation)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var animId = await stream.ReadUInt32Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            if (!_animations.ContainsKey(animId))
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidAnimationId);
                                continue;
                            }

                            var animation = _animations[animId];
                            channelInfo.Animation = animation;
                            await channel.SetAnimationAsync(
                                (Animation)Activator.CreateInstance(animation.GetAnimation()));

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSetChannelAnimationEnabled)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var enabled = await stream.ReadBooleanAsync();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            if (channel.Animation == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.AnimationNotSet);
                                continue;
                            }

                            await channel.Animation.SetEnabledAsync(enabled);

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSetChannelAnimationSpeed)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var speed = await stream.ReadFloatAsync();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            if (channel.Animation == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.AnimationNotSet);
                                continue;
                            }

                            await channel.Animation.SetSpeedAsync(speed);

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSetChannelAnimationColorCount)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var count = await stream.ReadUInt8Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            if (channel.Animation == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.AnimationNotSet);
                                continue;
                            }

                            await channel.Animation.SetColorCountAsync(count);

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSetChannelAnimationColor)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();
                            var index = await stream.ReadUInt8Async();
                            var r = await stream.ReadUInt8Async();
                            var g = await stream.ReadUInt8Async();
                            var b = await stream.ReadUInt8Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            if (channel.Animation == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.AnimationNotSet);
                                continue;
                            }

                            await channel.Animation.SetColorAsync(index, new Color(r, g, b));

                            await stream.WriteUInt8Async((byte)Result.Ok);
                        }
                        else if (command == (byte)Command.DeviceSendChannelAnimationRequest)
                        {
                            var deviceId = await stream.ReadUInt32Async();
                            var channelIndex = await stream.ReadUInt8Async();

                            var device = _devices.Where(pair => pair.Key.Id == deviceId).Select(pair => pair.Key).First();
                            if (device == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidDeviceId);
                                continue;
                            }

                            var deviceInfo = _devices[device];
                            if (channelIndex > device.Channels.Length)
                            {
                                await stream.WriteUInt8Async((byte)Result.InvalidChannelIndex);
                                continue;
                            }

                            // Check if device is open
                            if (!device.IsOpen)
                            {
                                await stream.WriteUInt8Async((byte)Result.DeviceNotOpen);
                                continue;
                            }

                            var channel = device.Channels[channelIndex];
                            var channelInfo = deviceInfo.Channels[channel];
                            if (channelInfo.Client != client)
                            {
                                await stream.WriteUInt8Async((byte)Result.AccessDenied);
                                continue;
                            }

                            if (channel.Animation == null)
                            {
                                await stream.WriteUInt8Async((byte)Result.AnimationNotSet);
                                continue;
                            }

                            await stream.WriteUInt8Async((byte)Result.Ok);

                            await channelInfo.Animation.HandleRequestAsync(channel, stream, stream);
                        }
                    }
                }
                catch
                {
                    // Client disconnected
                    lock (_clients) _clients.RemoveAll(c => c.InternalClient == e.Client);
                    foreach (var devicePair in _devices)
                    {
                        foreach (var channelPair in devicePair.Value.Channels)
                        {
                            var channelInfo = channelPair.Value;
                            if (channelInfo.Client != null && channelInfo.Client.InternalClient == e.Client)
                            {
                                channelInfo.Client = null;
                                channelInfo.Mutex.Unlock();

                                await Log.InfoAsync($"Releasing channel from disconnected client: {e.Source}");
                            }
                        }

                        var deviceInfo = devicePair.Value;
                        if (deviceInfo.Client != null && deviceInfo.Client.InternalClient == e.Client)
                        {
                            deviceInfo.Client = null;
                            deviceInfo.Mutex.Unlock();

                            await Log.InfoAsync($"Releasing device from disconnected client: {e.Source}");
                        }
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
