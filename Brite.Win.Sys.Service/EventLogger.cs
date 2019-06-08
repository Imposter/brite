/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Brite.Utility.IO;

namespace Brite.Win.Sys.Service
{
    public class EventLogger : Logger
    {
        private readonly string _applicationName;
        private readonly EventLog _eventLog;

        public EventLogger(string applicationName, LoggerLevel level = LoggerLevel.Error)
            : base(level)
        {
            _applicationName = applicationName;

            if (!EventLog.SourceExists(applicationName))
                EventLog.CreateEventSource(applicationName, "Application");

            _eventLog = new EventLog
            {
                Source = applicationName,
                Log = "Application"
            };
        }

        public override Task WriteLineAsync(string format, params object[] args)
        {
            return Task.Run(() =>
            {
                _eventLog.WriteEntry(string.Format(format, args));
                Debug.WriteLine(format, args);
                Console.WriteLine(format, args);
            });
        }
    }
}
