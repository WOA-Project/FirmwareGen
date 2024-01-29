using CommandLine;

namespace FirmwareGen.CommandLine
{
    [Verb("generate-windows-ffu", HelpText = "Generates a ffu from a Windows Base VHDX")]
    public class GenerateWindowsFFUOptions
    {
        [Option('d', "driver-pack", HelpText = "Todo", Required = true)]
        public string DriverPack
        {
            get; set;
        }

        [Option('o', "output", HelpText = "Todo", Required = true)]
        public string Output
        {
            get; set;
        }

        [Option('i', "input", HelpText = "Todo", Required = true)]
        public string Input
        {
            get; set;
        }

        [Option('v', "windows-ver", HelpText = "Todo", Required = true)]
        public string WindowsVer
        {
            get; set;
        }
    }
}
