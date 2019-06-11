using Brite.Win.App.Core.Plugin;

namespace Brite.Win.App.Core
{
    class Profile
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public PluginInstance Plugin { get; set; }
    }
}
