﻿/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Brite.Micro;
using Brite.Micro.Programmers;
using Brite.Micro.Programmers.StkV1;
using Brite.Win.Core.IO.Serial;

namespace Brite.Win.Con.FirmwareUpdater
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Brite Firmware Updater v{0}", Assembly.GetExecutingAssembly().GetName().Version);

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // Check if file exists
                if (!File.Exists(options.FirmwareFile))
                {
                    Console.WriteLine("Firmware file not found!");
                    return;
                }

                UpdateFirmwareAsync(options).Wait();

                Console.WriteLine("All done");
                Thread.Sleep(1000);
            }
        }

        private static async Task UpdateFirmwareAsync(Options options)
        {
            // Read file
            var buffer = File.ReadAllBytes(options.FirmwareFile);

            // Create serial connection
            using (var serial = new SerialConnection(options.PortName, options.BaudRate))
            {
                using (var channel = new Channel(serial))
                {
                    // Set up device information for Atmel ATmega328 (basic Arduino)
                    var info = new DeviceInfo(DeviceType.ATmega328);
                    info.Ram.Size = 2048;
                    info.Flash.Size = 32768;
                    info.Flash.PageSize = 128;
                    info.Eeprom.Size = 1024;
                    info.Eeprom.PageSize = 1;
                    info.LockBits.Size = 1;
                    info.FuseBits.Size = 3;

                    // Create programmer using device information
                    using (var programmer = new StkV1Programmer(channel, info))
                    {
                        // Open a connection with the device
                        await programmer.OpenAsync();

                        // Write to device
                        await programmer.WriteAsync(MemoryType.Flash, buffer, 0, buffer.Length);

                        // Reset device
                        await programmer.ResetAsync();
                    }
                }
            }
        }
    }
}
