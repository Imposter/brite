using System.Collections.Generic;

namespace Brite.Win.Con.Daemon
{
    public class Config
    {
        public int Port { get; set; } = 6450;
        public int Timeout { get; set; } = 1000;
        public int Retries { get; set; } = 10;
        public Dictionary<string, uint> Devices { get; set; } = new Dictionary<string, uint>();
    }
}
