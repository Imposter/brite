using Brite.Utility.IO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Brite.Win.App.Core.Plugin;
using Brite.Win.App.Core.Scripting;

namespace Brite.UWP.App.Core.Plugin
{
    class PluginManager : IDisposable
    {
        private class PluginRegistry
        {
            public PluginInfo Info { get; }
            public string Path { get; }
            public Script Script { get; }
            public dynamic ClassObj { get; }

            public PluginRegistry(PluginInfo info, string path, Script script, dynamic classObj)
            {
                Info = info;
                Path = path;
                Script = script;
                ClassObj = classObj;
            }
        }

        private const string PluginsPath = "./core/plugins/";
        private const string LibsPath = "./core/libs/";

        private static readonly Log Log = Logger.GetLog<PluginManager>();

        private readonly Dictionary<string, PluginRegistry> _pluginRegistries;
        private readonly List<PluginInstance> _instances;

        private readonly IPAddress _ipAddress;
        private readonly int _port;

        public List<string> AvailablePlugins => _pluginRegistries.Keys.ToList();

        public PluginManager(IPAddress ipAddress, int port)
        {
            _pluginRegistries = new Dictionary<string, PluginRegistry>();
            _instances = new List<PluginInstance>();

            _ipAddress = ipAddress;
            _port = port;
        }

        public async Task InitializeAsync()
        {
            // Find existing plugins
            var pluginsFile = Path.Combine(PluginsPath, "plugins.dat");
            if (File.Exists(pluginsFile))
            {
                var paths = File.ReadAllLines(pluginsFile);
                foreach (var relativePath in paths)
                {
                    // Ignore comments
                    if (relativePath.StartsWith("#")) continue;

                    var pluginPath = Path.GetFullPath(Path.Combine(PluginsPath, relativePath));
                    if (!Directory.Exists(pluginPath))
                    {
                        await Log.WarnAsync($"Plugin \"{pluginPath}\" not found, ignoring plugin...");
                        continue;
                    }

                    var pluginConfigPath = Path.Combine(pluginPath, "plugin.json");
                    if (!File.Exists(pluginConfigPath))
                    {
                        await Log.WarnAsync($"Plugin \"{pluginPath}\" info not found, ignoring plugin...");
                        continue;
                    }

                    var info = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(pluginConfigPath));

                    // Ensure the info contains a valid script entrypoint
                    var pluginSourcePath = Path.Combine(pluginPath, "__init__.py");
                    if (!File.Exists(pluginSourcePath))
                    {
                        await Log.WarnAsync($"Plugin \"{pluginPath}\" source entrypoint not found, ignoring plugin...");
                        continue;
                    }

                    // Initialize plugin
                    var pluginCode = File.ReadAllText(pluginSourcePath);
                    var script = new Script(pluginCode);

                    // Add search paths
                    script.AddSearchPath(LibsPath);
                    script.AddSearchPath(pluginPath);

                    // Set RegisterPlugin so we can register IPlugin type from python
                    dynamic classObj = null;
                    script.SetGlobalVariable("RegisterPlugin", new Action<dynamic>(obj => { classObj = obj; }));

                    // Execute plugin
                    await script.ExecuteAsync();

                    // Check if plugin class was registered
                    if (classObj == null)
                    {
                        await Log.WarnAsync($"Plugin \"{pluginPath}\" class not registered, ignoring plugin...");
                        continue;
                    }

                    _pluginRegistries.Add(info.Name, new PluginRegistry(info, pluginPath, script, classObj));

                    await Log.InfoAsync($"Loaded plugin {info.Name} (v{info.Version})");
                }
            }
        }

        public async Task ShutdownAsync()
        {
            // Dispose instances
            foreach (var instance in _instances)
                instance.Dispose();
            _instances.Clear();
        }

        public async Task<PluginInstance> CreatePluginInstanceAsync(string name, string id, IDictionary<string, dynamic> options = null)
        {
            if (!_pluginRegistries.ContainsKey(name))
                throw new ArgumentException("Invalid plugin name specified");

            // Get info
            var registry = _pluginRegistries[name];
            var config = registry.Info;
            var path = registry.Path;
            var script = registry.Script;
            var classObj = registry.ClassObj;

            var localOptions = new Dictionary<string, dynamic>
            {
                { "__id__", id },
                { "__path__", path },
                { "__ipAddress__", _ipAddress },
                { "__port__", _port }
            };
            if (options != null)
            {
                // Merge
                foreach (var option in options)
                {
                    if (localOptions.ContainsKey(option.Key))
                        localOptions[option.Key] = option.Value;
                    else localOptions.Add(option.Key, option.Value);
                }
            }

            // Initialize plug
            var plugin = script.CreateInstance(classObj, localOptions);

            // Create instance
            var instance = new PluginInstance(plugin, config);
            _instances.Add(instance);

            await Log.InfoAsync($"Created instance of {config.Name} (v{config.Version})");

            return instance;
        }

        public void Dispose()
        {
            // Dispose instances
            foreach (var instance in _instances)
                instance.Dispose();
            _instances.Clear();

            // Dispose registry scripts
            foreach (var registry in _pluginRegistries)
                registry.Value.Script.Dispose();
            _pluginRegistries.Clear();
        }
    }
}
