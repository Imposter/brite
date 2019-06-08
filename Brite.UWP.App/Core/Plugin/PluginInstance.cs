using Brite.UWP.App.Plugin;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Brite.UWP.App.Core.Plugin
{
    class PluginInstance : IDisposable
    {
        private readonly IPlugin _plugin;
        private readonly PluginInfo _info;

        public PluginInfo Info => _info;
        public Page ConfigPage => _plugin.GetConfigPage();

        public PluginInstance(IPlugin plugin, PluginInfo info)
        {
            _plugin = plugin;
            _info = info;
        }

        public async Task StartAsync()
        {
            await Task.Run(() => _plugin.Start());
        }

        public async Task StopAsync()
        {
            await Task.Run(() =>_plugin.Stop());
        }

        public void Dispose()
        {
            _plugin.Dispose();
        }
    }
}
