﻿namespace Brite.Utility.IO
{
    public static class Logger
    {
        public static Log GetLog<T>()
        {
            return new Log(typeof(T).GetFriendlyName());
        }

        public static Log GetLog()
        {
            return new Log("Application");
        }
    }
}