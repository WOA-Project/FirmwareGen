using CommandLine;

namespace FirmwareGen.CommandLine
{
    [Verb("generate-windows-ffu", HelpText = "Generates a ffu from a Windows Base VHDX")]
    public class GenerateWindowsFFUOptions
    {
        [Option('d', "driver-pack", HelpText = "Driver pack with device.xml", Required = true)]
        public string DriverPack
        {
            get; set;
        }

        [Option('o', "output", HelpText = "Output folder", Required = true)]
        public string Output
        {
            get; set;
        }

        [Option('v', "windows-ver", HelpText = "Version of Windows e.g. 10.0.26063.1", Required = true)]
        public string WindowsVer
        {
            get; set;
        }

        [Option('w', "windows-dvd", HelpText = "Windows Setup Media DVD drive letter e.g. F:", Required = true)]
        public string WindowsDVD
        {
            get; set;
        }

        [Option('i', "windows-index", HelpText = "The index of the Windows Setup Media DVD Install Image, to apply. e.g. 1", Required = true)]
        public string WindowsIndex
        {
            get; set;
        }

        [Option('p', "device-profile", HelpText = "Path to the device profile xml containing information on how to build the FFU", Required = true)]
        public string DeviceProfile
        {
            get; set;
        }

        [Option('t', "secure-boot-signing-command", HelpText = "Optional command to sign the resulting FFU file to work in Secure Boot scenarios. The File to sign is passed as a path appended after the provided command string, and is the sole and only argument provided.", Default = "", Required = false)]
        public required string SecureBootSigningCommand
        {
            get; set;
        }
    }
}
