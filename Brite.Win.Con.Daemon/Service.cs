﻿using Brite.API;
using Brite.API.Animations.Server;
using Brite.Utility.IO;
using Brite.Win.Core.Hardware.Serial;
using Brite.Win.Core.IO.Serial;
using Brite.Win.Core.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Brite.Win.Con.Daemon
{
    internal class Service
    {
        private static readonly Log Log = Logger.GetLog<Service>(); // TODO: Use logging

        private readonly string _path;
        private readonly List<Device> _devices;
        private BriteServer _server;
        private DateTime _lastConfigModifiedTime;
        private bool _running;
        private Thread _thread;

        public Service(string path)
        {
            _path = path;
            _devices = new List<Device>();
        }

        public async Task StartAsync()
        {
            if (_running)
                throw new InvalidOperationException("Already running");

            await LoadConfigAsync();

            _running = true;

            _thread = new Thread(async () =>
            {
                while (_running)
                {
                    // Load config if changed
                    await LoadConfigAsync();

                    Thread.Sleep(1);
                }
            });

            _thread.Start();
        }

        public async Task StopAsync()
        {
            if (!_running)
                throw new InvalidOperationException("Not running");

            _running = false;
            _thread.Join();

            // stop server
            await _server.StopAsync();

            // disconnect devices
            foreach (var device in _devices)
                await device.CloseAsync();
        }

        private async Task LoadConfigAsync()
        {
            var configFile = Path.GetFullPath(Path.Combine(_path, ".\\config.json"));
            var modifiedTime = File.GetLastWriteTime(configFile);
            if (modifiedTime == _lastConfigModifiedTime)
                return;

            if (_server != null && _server.Running)
                await _server.StopAsync();

            // close devices
            foreach (var device in _devices)
                await device.CloseAsync();

            _devices.Clear();

            try
            {
                Config config;
                using (var stream = new FileStream(configFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        config = JsonConvert.DeserializeObject<Config>(await reader.ReadToEndAsync());

                        // Find devices
                        var deviceSearcher = new SerialDeviceSearcher();
                        var discoveredDevices = await Device.GetDevicesAsync<SerialConnection>(deviceSearcher);

                        // Initialize devices
                        foreach (var device in discoveredDevices)
                        {
                            if (!config.Devices.ContainsKey(device.Info.PortName))
                                continue;

                            // Connect to device
                            var baudRate = config.Devices[device.Info.PortName];
                            await device.OpenAsync(baudRate, config.Timeout, config.Retries); // TODO/NOTE: We need to fix connections to devices

                            _devices.Add(device);
                        }

                        reader.Close();
                    }
                }

                // Create underlying server
                var tcpServer = new TcpServer(new IPEndPoint(IPAddress.Any, config.Port));

                _server = new BriteServer(tcpServer);
                _server.AddDevices(_devices);

                // Add animations
                _server.AddAnimation(new ManualAnimation());
                _server.AddAnimation(new BreatheAnimation());
                
                await _server.StartAsync();
            }
            catch (Exception ex)
            {
                await Log.ErrorAsync("LoadConfigAsync (FAILED): {0}", ex);
                return;
            }

            _lastConfigModifiedTime = modifiedTime;
        }

        private static void LoadAssembliesInDirectory(string path)
        {
            var files = Directory.GetFiles(path, "*.dll");
            foreach (var file in files)
                Assembly.LoadFile(file);
        }
    }
}
