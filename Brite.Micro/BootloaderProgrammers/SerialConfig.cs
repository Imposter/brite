namespace Brite.Micro.BootloaderProgrammers
{
    public class SerialConfig
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public int ReadTimeOut { get; set; }
        public int WriteTimeOut { get; set; }

        private const int DefaultTimeout = 1000;

        public SerialConfig(string portName, int baudRate,
            int readTimeout = DefaultTimeout, int writeTimeout = DefaultTimeout)
        {
            PortName = portName;
            BaudRate = baudRate;
            ReadTimeOut = readTimeout;
            WriteTimeOut = writeTimeout;
        }
    }
}
