﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Brite.Win.Con.Daemon
{
    public class Logger : Utility.IO.Logger
    {
        private readonly FileStream _stream;

        public Logger(string fileName)
        {
            _stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        public override async Task WriteLineAsync(string format, params object[] args)
        {
            var line = string.Format(format, args) + Environment.NewLine;
            await _stream.WriteAsync(Encoding.UTF8.GetBytes(line), 0, line.Length);
            Console.Write(line);
        }
    }
}
