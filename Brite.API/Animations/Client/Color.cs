namespace Brite.API.Animations.Client
{
    public class Color
    {
        public static readonly Color Black = new Color(0, 0, 0);

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        public int Rgb => ((R & 0xFF) << 16) | ((G & 0xFF) << 8) | (B & 0xFF);

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color(int r, int g, int b)
        {
            R = (byte) r;
            G = (byte) g;
            B = (byte) b;
        }

        public Color(int rgb)
        {
            R = (byte)(rgb >> 16 & 0xFF);
            G = (byte)(rgb >> 8 & 0xFF);
            B = (byte)(rgb & 0xFF);
        }
    }
}
