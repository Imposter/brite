/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

using Brite.API.Animations.Server;
using Brite.API.Server;
using Brite.Utility.IO;
using Brite.Utility.Network;
using Brite.Win.Core.Hardware.Serial;
using Brite.Win.Core.IO.Serial;
using Brite.Win.Core.Network;

using Newtonsoft.Json;

namespace Brite.Win.Sys.Service
{
    public class BriteService : ServiceBase
    {
        private const int ConfigCheckDelay = 10000;
        private const string ConfigFileName = "config.json";

        private static Log Log = Logger.GetLog();

        private string _instancePath;
        private Config _config;
        private ITcpServer _server;
        private BriteServer _briteServer;
        private IEnumerable<Device> _devices;
        private Thread _changeThread;
        private bool _running;
        private DateTime _lastModified;

        protected override async void OnStart(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif

            // Initialize logger
            var logger = new EventLogger("BriteService",
#if DEBUG
                LoggerLevel.Trace
#else
                LoggerLevel.Error
#endif
            );
            Logger.SetInstance(logger);

            // Read instance
            _instancePath = AppDomain.CurrentDomain.BaseDirectory;
            if (args.Length > 1)
                _instancePath = args[1];

            // Check if instance path exists
            if (!Directory.Exists(_instancePath))
            {
                await Log.ErrorAsync("Invalid instance path!");
                Environment.Exit(-1);
            }

            // Check if config file exists
            if (!File.Exists(Path.Combine(_instancePath, ConfigFileName)))
            {
                await Log.ErrorAsync("Config file not found!");
                Environment.Exit(-1);
            }

            // Log uncaught exceptions
            AppDomain.CurrentDomain.UnhandledException += async (sender, eventArgs) =>
                await Logger.GetLog().ErrorAsync(eventArgs.ExceptionObject.ToString());

            // Set as running
            _running = true;

            _changeThread = new Thread(ChangeThreadProcess);
            _changeThread.Start();
        }

        protected override async void OnStop()
        {
            _running = false;
            _changeThread.Join();

            // TODO: OnShutdown, memorize settings and reapply them when the service starts -- useful for windows shutdown/startup?

            // Stop previously active server
            if (_briteServer != null)
                await _briteServer.StopAsync();
        }

        protected override void OnShutdown()
        {
            if (_running)
                OnStop();
        }

        private async void ChangeThreadProcess()
        {
            while (_running)
            {
                // Check if config was modified
                var path = Path.Combine(_instancePath, ConfigFileName);
                var modifiedTime = File.GetLastWriteTime(path);
                if (modifiedTime == _lastModified)
                {
                    Thread.Sleep(ConfigCheckDelay);
                    continue;
                }

                // Stop previously active server
                if (_briteServer != null)
                    await _briteServer.StopAsync();

                // Read config
                _config = await LoadConfigAsync(path);

                // Find devices
                _devices = await GetDevicesAsync(_config.Devices.Keys);

                // Create underlying server
                _server = new TcpServer(new IPEndPoint(IPAddress.Any, _config.Port));

                // Create server
                _briteServer = new BriteServer(_server, _config.Timeout, _config.Retries, _config.ConnectionRetries);

                // Add animations
                _briteServer.AddAnimations(new BaseAnimation[]
                {
                    new ManualAnimation(),
                    new FixedAnimation(),
                    new BreatheAnimation(),
                    new PulseAnimation(),
                    new FadeAnimation(),
                    new MarqueeAnimation(),
                    new SpiralAnimation()
                });

                // Add devices
                foreach (var device in _devices)
                {
                    var baudRate = _config.Devices[device.Info.PortName];
                    _briteServer.AddDevice(device, baudRate);
                }

                // Start server
                await _briteServer.StartAsync();

                // Update previously modified time
                _lastModified = modifiedTime;
            }
        }

        private static async Task<Config> LoadConfigAsync(string path)
        {
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var reader = new StreamReader(stream);

            var data = await reader.ReadToEndAsync();
            var config = JsonConvert.DeserializeObject<Config>(data);

            reader.Close();
            stream.Close();

            return config;
        }

        private static async Task<IEnumerable<Device>> GetDevicesAsync(IEnumerable<string> ports)
        {
            // Find devices
            var deviceSearcher = new SerialDeviceSearcher();
            var discoveredDevices = await Device.GetDevicesAsync<SerialConnection>(deviceSearcher);

            return discoveredDevices.Where(device => ports.Contains(device.Info.PortName));
        }
    }
}
