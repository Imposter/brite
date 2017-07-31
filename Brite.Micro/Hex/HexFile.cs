using System;
using System.Collections.Generic;
using System.IO;

namespace Brite.Micro.Hex
{
    public class HexFile
    {
        private readonly List<HexFileLine> _lines = new List<HexFileLine>();

        public List<HexFileLine> Lines => _lines;

        public int CodeSize
        {
            get
            {
                var val1 = 0;
                foreach (var line in _lines)
                {
                    if (line.Type == HexFileLineType.Data)
                        val1 = Math.Max(val1, line.Address + line.Data.Length);
                }
                return val1;
            }
        }

        public byte[] GetCode()
        {
            var numArray = new byte[CodeSize];
            foreach (var line in _lines)
            {
                if (line.Type == HexFileLineType.Data)
                {
                    for (var index = 0; index < line.Data.Length; ++index)
                        numArray[line.Address + index] = line.Data[index];
                }
            }
            return numArray;
        }

        public static HexFile Parse(TextReader reader)
        {
            var hexFile = new HexFile();
            var num = 1;
            try
            {
                while (reader.Peek() != -1)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var hexFileLine = HexFileLine.Parse(line);
                        hexFile.Lines.Add(hexFileLine);
                    }
                    ++num;
                }
            }
            catch (Exception ex)
            {
                throw new HexFileException("couldn't parse line " + num + ". " + ex.Message);
            }
            return hexFile;
        }
    }
}
