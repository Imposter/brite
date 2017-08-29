using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public abstract class Logger
    {
        private static Logger Instance = new DebugLogger();

        public abstract Task WriteLineAsync(string format, params object[] args);

        public static void SetInstance(Logger logger)
        {
            Instance = logger;
        }

        public static Log GetLog<T>()
        {
            return new Log(ref Instance, typeof(T).GetFriendlyName());
        }

        public static Log GetLog()
        {
            return new Log(ref Instance, "Application");
        }
    }
}
