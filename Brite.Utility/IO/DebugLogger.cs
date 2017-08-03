using System.Diagnostics;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public class DebugLogger : Logger
    {
        public DebugLogger()
        {
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task WriteLineAsync(string format, params object[] args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Debug.WriteLine(format, args);
        }
    }
}