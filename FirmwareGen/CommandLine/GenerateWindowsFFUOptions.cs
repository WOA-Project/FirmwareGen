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

        [Option('v', "windows-ver", HelpText = "Todo", Required = true)]
        public string WindowsVer
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

        [Option('p', "device-profile", HelpText = "Path to the device profile xml containing information on how to build the FFU", Required = true)]
        public string DeviceProfile
        {
            get; set;
        }
    }
}
