using System.Text;

namespace Brite.Utility.Crypto
{
    public static class Hash
    {
        private const uint Fnv1APrime32 = 0x01000193;
        private const uint Fnv1AOffset32 = 0x811C9DC5;

        public static uint Fnv1A32(byte[] buffer)
        {
            var offset = Fnv1AOffset32;
            foreach (var b in buffer)
            {
                offset *= Fnv1APrime32;
                offset ^= b;
            }

            return offset;
        }

        public static uint Fnv1A32(string str)
        {
            return Fnv1A32(Encoding.ASCII.GetBytes(str));
        }
    }
}
