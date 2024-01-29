using CommandLine;

namespace FirmwareGen.CommandLine
{
    [Verb("generate-windows-base-vhdx", HelpText = "Generates a Windows Base VHDX")]
    public class GenerateWindowsOptions
    {
        [Option('o', "output", HelpText = "Todo", Required = true)]
        public string Output
        {
            get; set;
        }

        [Option('w', "windows-dvd", HelpText = "Todo", Required = true)]
        public string WindowsDVD
        {
            get; set;
        }

        [Option('i', "windows-index", HelpText = "Todo", Required = true)]
        public string WindowsIndex
        {
            get; set;
        }
    }
}
