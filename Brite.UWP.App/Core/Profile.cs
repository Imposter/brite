using Brite.UWP.App.Core.Plugin;

namespace Brite.UWP.App.Core
{
    class Profile
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public PluginInstance Plugin { get; set; }
    }
}
