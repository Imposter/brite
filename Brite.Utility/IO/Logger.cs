/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public abstract class Logger
    {
        private static Logger _instance = new DebugLogger();
        
        public LoggerLevel Level { get; set; }
        public abstract Task WriteLineAsync(string format, params object[] args);

        public Logger(LoggerLevel level = LoggerLevel.Error)
        {
            Level = level;
        }

        public static void SetInstance(Logger logger)
        {
            _instance = logger;
        }

        public static Log GetLog<T>()
        {
            return new Log(ref _instance, typeof(T).GetFriendlyName());
        }

        public static Log GetLog()
        {
            return new Log(ref _instance, "Application");
        }
    }
}
