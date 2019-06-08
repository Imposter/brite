using Windows.UI.Xaml.Markup;

namespace Brite.UWP.App.Plugin.Helpers
{
    public sealed class XamlHelper
    { 
        public static object Load(string data)
        {
            return XamlReader.Load(data);
        }
    }
}
