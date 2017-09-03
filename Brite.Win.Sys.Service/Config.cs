using System.Collections.Generic;

namespace Brite.Win.Sys.Service
{
    public class Config
    {
        public int Port { get; set; } = 6450;
        public int Timeout { get; set; } = 150;
        public int Retries { get; set; } = 10;
        public int ConnectionRetries { get; set; } = 5;
        public Dictionary<string, uint> Devices { get; set; } = new Dictionary<string, uint>();
    }
}
