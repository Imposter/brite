using D = System.Diagnostics.Debug;

namespace Brite.Utility.IO
{
    public class Log
    {
        private readonly string _name;

        public Log(string name)
        {
            _name = name;
        }

        public void Info(string format, params object[] args)
        {
            D.WriteLine("[INFO] {0}: {1}", _name, string.Format(format, args));
        }

        public void Error(string format, params object[] args)
        {
            D.WriteLine("[ERROR] {0}: {1}", _name, string.Format(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            D.WriteLine("[WARN] {0}: {1}", _name, string.Format(format, args));
        }

        public void Debug(string format, params object[] args)
        {
            D.WriteLine("[DEBUG] {0}: {1}", _name, string.Format(format, args));
        }

        public void Trace(string format, params object[] args)
        {
            D.WriteLine("[TRACE] {0}: {1}", _name, string.Format(format, args));
        }
    }
}
