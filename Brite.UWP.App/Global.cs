using Brite.API;
using Brite.Utility.Network;

namespace Brite.UWP.App
{
    internal static class Global
    {
        public static bool Running;

        public static RootFrame RootFrame;

        public static ITcpClient TcpClient;
        public static BriteClient BriteClient;

        public static Config Config;
    }
}