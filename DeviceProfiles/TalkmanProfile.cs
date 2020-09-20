using System.IO;
using System.Linq;

namespace FirmwareGen.DeviceProfiles
{
    class TalkmanProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\950_Broad_Availability.bin";
        }

        public string DriverCommand(string DriverFolder)
        {
            string definitionFile = $@"{DriverFolder}\definitions\950.txt";
            string[] definitionPaths = File.ReadAllLines(definitionFile).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string driverCommand = $"{string.Join(" ", definitionPaths.Select(x => $"/Driver:\"{DriverFolder}\\{x}\""))} /Recurse";
            return driverCommand;
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"{OSVersion}_CLIENT{Sku}_TALKMAN_A64FRE_{Language}.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8992.P6218";
        }

        public string[] SupplementaryBCDCommands()
        {
            return new string[0];
        }
    }
}
