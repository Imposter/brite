using Brite.API;
using Brite.API.Animations.Server;
using Brite.Utility.IO;
using Brite.Utility.Network;
using Brite.Win.Core.Hardware.Serial;
using Brite.Win.Core.IO.Serial;
using Brite.Win.Core.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Brite.Win.Sys.Service
{
    public class BriteService : ServiceBase
    {
        private const int ConfigCheckDelay = 10000;
        private const string ConfigFileName = "config.yml";

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

            // Stop previously active server
            if (_briteServer != null)
                await _briteServer.StopAsync();

            // Stop previously active devices
            if (_devices != null)
                foreach (var device in _devices)
                    await device.CloseAsync();
        }

        private async void ChangeThreadProcess()
        {
            while (_running)
            {
                Thread.Sleep(ConfigCheckDelay);

                // Check if config was modified
                var path = Path.Combine(_instancePath, ConfigFileName);
                var modifiedTime = File.GetLastWriteTime(path);
                if (modifiedTime == _lastModified)
                    continue;

                // Stop previously active server
                if (_briteServer != null)
                    await _briteServer.StopAsync();

                // Stop previously active devices
                if (_devices != null)
                    foreach (var device in _devices)
                        await device.CloseAsync();

                // Read config
                _config = await LoadConfigAsync(path);

                // Find devices
                _devices = await GetDevicesAsync(_config.Devices.Keys);

                // Initialize devices
                foreach (var device in _devices)
                {
                    for (var i = 0; i < _config.ConnectionRetries; i++)
                    {
                        try
                        {
                            var baudRate = _config.Devices[device.Info.PortName];
                            await device.OpenAsync(baudRate, _config.Timeout, _config.Retries, true);
                        }
                        catch (Exception ex)
                        {
                            if (i != _config.ConnectionRetries - 1)
                            {
                                await Log.ErrorAsync($"[{i + 1}] Failed to connect to device {device.Info.PortName}, retrying: {ex}");
                            }
                            else
                            {
                                await Log.ErrorAsync($"[{i + 1}] Failed to connect to device {device.Info.PortName}, ignoring device: {ex}");
                                break;
                            }
                        }
                    }
                }

                // Create underlying server
                _server = new TcpServer(new IPEndPoint(IPAddress.Any, _config.Port));

                // Create server
                _briteServer = new BriteServer(_server);

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
                _briteServer.AddDevices(_devices);

                // Start server
                await _briteServer.StartAsync();

                // Update previously modified time
                _lastModified = modifiedTime;
            }
        }

        private static async Task<Config> LoadConfigAsync(string path)
        {
            return await Task.Run(() =>
            {
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var reader = new StreamReader(stream);
                var deserializer = new Deserializer();

                var config = deserializer.Deserialize<Config>(reader);

                reader.Close();
                stream.Close();

                return config;
            });
        }

        private static async Task<IEnumerable<Device>> GetDevicesAsync(IEnumerable<string> ports)
        {
            // Find devices
            var deviceSearcher = new SerialDeviceSearcher();
            var discoveredDevices = await Device.GetDevicesAsync<SerialConnection>(deviceSearcher);

            return discoveredDevices.Where(device => ports.Contains(device.Info.PortName));
        }

        private void InitializeComponent()
        {
            // 
            // BriteService
            // 
            this.ServiceName = "BriteService";

        }
    }
}
