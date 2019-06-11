using System.IO;
using System.Text;
using System.Windows.Markup;

namespace Brite.Win.App.Plugin.Helpers
{
    public sealed class XamlHelper
    { 
        public static object Load(string data)
        {
            return XamlReader.Load(new MemoryStream(Encoding.ASCII.GetBytes(data)));
        }
    }
}
