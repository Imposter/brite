using Brite.Win.Core.IO;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Win.Con.IntelToBinary
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Brite Intel HEX to Binary Converter v{0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Copyright (C) 2017 Indigo Games. All Rights Reserved.");
            Console.WriteLine();

            foreach (var file in args)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine($"{file} does not exist!");
                    continue;
                }

                try
                {
                    ConvertFileAsync(file).Wait();
                    Console.WriteLine($"Converted {file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to convert {file}");
                    Console.WriteLine(ex);
                }
            }

            Console.WriteLine("All done!");
            Thread.Sleep(1000);
        }

        private static async Task ConvertFileAsync(string filePath)
        {
            // Create output file name
            var outputFile = Path.GetDirectoryName(filePath) + "/" + Path.GetFileNameWithoutExtension(filePath) + ".bin";
            
            using (var input = new TimedStream(File.OpenRead(filePath)))
            {
                using (var output = File.Open(outputFile, FileMode.Create))
                {
                    var inputStream = new BinaryStream(input);
                    var newlineCharacters = Environment.NewLine.ToCharArray();

                    while (true)
                    {
                        char recordMark;
                        try
                        {
                            recordMark = await inputStream.ReadCharAsync();
                        }
                        catch (TimeoutException)
                        {
                            break;
                        }

                        if (newlineCharacters.Contains(recordMark))
                            continue;

                        if (recordMark != ':')
                            throw new InvalidDataException("Invalid record mark");

                        var recordLength = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);
                        var recordOffset = ushort.Parse(await inputStream.ReadStringAsync(4), NumberStyles.AllowHexSpecifier);
                        var recordType = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);

                        var calculatedChecksum = (byte)(recordLength + recordType + (byte)recordOffset + (byte)((recordOffset & 0xFF00) >> 8));

                        var recordData = new byte[recordLength];
                        for (var i = 0; i < recordLength; i++)
                        {
                            recordData[i] = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);
                            calculatedChecksum += recordData[i];
                        }

                        var recordChecksum = byte.Parse(await inputStream.ReadStringAsync(2), NumberStyles.AllowHexSpecifier);

                        // Finalize checksum
                        calculatedChecksum = (byte)(~calculatedChecksum + 1);
                        if (recordChecksum != calculatedChecksum)
                            throw new InvalidDataException("Invalid checksum value");

                        await output.WriteAsync(recordData, 0, recordData.Length);
                    }
                }
            }
        }
    }
}
