﻿/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using Brite.Utility.IO;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace Brite.UWP.App
{
    internal class FileLogger : Logger
    {
        private readonly StorageFile _file;

        public FileLogger(StorageFile file)
        {
            _file = file;
        }

        public override async Task WriteLineAsync(string format, params object[] args)
        {
            await Task.Run(() => Debug.WriteLine(format, args));
            await FileIO.WriteTextAsync(_file, string.Format(format, args) + Environment.NewLine, Windows.Storage.Streams.UnicodeEncoding.Utf8);
        }
    }
}