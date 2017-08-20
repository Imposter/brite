using CommandLine;
using CommandLine.Text;

namespace Brite.Win.Con.FirmwareUpdater
{
    public class Options
    {
        [Option('p', "port", Required = true,
            HelpText = "COM port occupied by Brite device.")]
        public string PortName { get; set; }

        [Option('b', "baud", Required = true,
            HelpText = "Baud rate required by Brite device.")]
        public uint BaudRate { get; set; }

        [Option('f', "file", Required = true,
            HelpText = "Brite firmware file.")]
        public string FirmwareFile { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
