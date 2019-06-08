/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System.Diagnostics;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public class DebugLogger : Logger
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task WriteLineAsync(string format, params object[] args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            Debug.WriteLine(format, args);
        }
    }
}
