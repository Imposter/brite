using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Brite.Micro.Devices
{
    public static class DeviceInfoUtils
    {
        public static string FormatDescription(XElement xDescription)
        {
            var val = xDescription?.Value;
            if (string.IsNullOrWhiteSpace(val)) return null;
            return string.Join("\r\n", val.Trim()
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray()
            );
        }

        public static int ParseInt(string val)
        {
            if (val.StartsWith("0x")) return int.Parse(val.Substring(2), NumberStyles.HexNumber);
            return int.Parse(val);
        }

        public static bool TryParseInt(string s, out int result)
        {
            if (s.StartsWith("0x")) return int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
            return int.TryParse(s, out result);
        }

        public static bool TryParseUInt(string s, out uint result)
        {
            if (s.StartsWith("0x")) return uint.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
            return uint.TryParse(s, out result);
        }
    }
}
