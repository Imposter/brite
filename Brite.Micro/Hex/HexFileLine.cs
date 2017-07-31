using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Brite.Micro.Hex
{
    public class HexFileLine
    {
        public ushort Address { get; set; }
        public byte[] Data { get; set; }
        public HexFileLineType Type { get; set; }

        public byte Checksum
        {
            get
            {
                return (byte)(256 - GetLineBytes().Sum(bt => bt) & byte.MaxValue);
            }
        }

        private byte[] GetLineBytes()
        {
            var byteList = new List<byte>
            {
                Data != null ? (byte) Data.Length : (byte) 0,
                (byte) (Address >> 8 & byte.MaxValue),
                (byte) (Address & (uint) byte.MaxValue),
                (byte) Type
            };
            if (Data != null)
                byteList.AddRange(Data);
            return byteList.ToArray();
        }

        public static HexFileLine Parse(string line)
        {
            if (!line.StartsWith(":"))
                throw new ArgumentException();
            var start = 1;
            var num1 = ReadHexByte(ref line, ref start);
            var num2 = ReadHexUShort(ref line, ref start);
            var hexFileLineType = (HexFileLineType)ReadHexByte(ref line, ref start);
            var numArray = new byte[num1];
            for (var index = 0; index < num1; ++index)
                numArray[index] = ReadHexByte(ref line, ref start);
            var num3 = ReadHexByte(ref line, ref start);
            var hexFileLine = new HexFileLine
            {
                Address = num2,
                Data = numArray,
                Type = hexFileLineType
            };
            if (hexFileLine.Checksum != num3)
                throw new Exception("checksum mismatch");
            return hexFileLine;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(":");
            foreach (var lineByte in GetLineBytes())
                stringBuilder.Append(FormatByte(lineByte));
            stringBuilder.Append(FormatByte(Checksum));
            return stringBuilder.ToString();
        }

        private static string FormatByte(byte bt)
        {
            return "0123456789ABCDEF"[bt >> 4].ToString() + "0123456789ABCDEF"[bt & 15];
        }

        private static ushort ReadHexUShort(ref string line, ref int start)
        {
            return (ushort)(((uint)ReadHexByte(ref line, ref start) << 8) + ReadHexByte(ref line, ref start));
        }

        private static byte ReadHexByte(ref string line, ref int start)
        {
            return (byte)(((uint)GetHexDigit(line[start++]) << 4) + GetHexDigit(line[start++]));
        }

        private static byte GetHexDigit(char ch)
        {
            if (ch >= 48 && ch <= 57)
                return (byte)(ch - 48U);
            if (ch >= 65 && ch <= 90)
                return (byte)(ch - 65 + 10);
            if (ch >= 97 && ch <= 122)
                return (byte)(ch - 97 + 10);
            throw new ArgumentException("Invalid character " + ch);
        }
    }
}
