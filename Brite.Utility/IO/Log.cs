using System;
using D = System.Diagnostics.Debug;

namespace Brite.Utility.IO
{
    public static class Log
    {
        public static void Info(string format, params object[] args)
        {
            D.WriteLine(string.Format("[INFO] Application: {0}", string.Format(format, args)));
        }

        public static void Info<T>(string format, params object[] args)
        {
            D.WriteLine("[INFO] {0}: {1}", nameof(T), string.Format(format, args));
        }

        public static void Error(string format, params object[] args)
        {
            D.WriteLine(string.Format("[ERROR] Application: {0}", string.Format(format, args)));
        }

        public static void Error<T>(string format, params object[] args)
        {
            D.WriteLine("[ERROR] {0}: {1}", nameof(T), string.Format(format, args));
        }

        public static void Warn(string format, params object[] args)
        {
            D.WriteLine(string.Format("[WARN] Application: {0}", string.Format(format, args)));
        }

        public static void Warn<T>(string format, params object[] args)
        {
            D.WriteLine("[WARN] {0}: {1}", nameof(T), string.Format(format, args));
        }

        public static void Debug(string format, params object[] args)
        {
            D.WriteLine(string.Format("[DEBUG] Application: {0}", string.Format(format, args)));
        }

        public static void Debug<T>(string format, params object[] args)
        {
            D.WriteLine("[DEBUG] {0}: {1}", nameof(T), string.Format(format, args));
        }
    }
}
