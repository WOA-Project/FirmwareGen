using System.IO;
using System.Linq;

namespace FirmwareGen.DeviceProfiles
{
    class CitymanProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\950XL_Broad_Availability.bin";
        }

        public string DriverCommand(string DriverFolder)
        {
            string definitionFile = $@"{DriverFolder}\definitions\950xl.txt";
            string[] definitionPaths = File.ReadAllLines(definitionFile).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string driverCommand = $"{string.Join(" ", definitionPaths.Select(x => $"/Driver:\"{DriverFolder}\\{x}\""))} /Recurse";
            return driverCommand;
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"{OSVersion}_CLIENT{Sku}_CITYMAN_A64FRE_{Language}.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8994.P6211";
        }

        public string[] SupplementaryBCDCommands()
        {
            return new string[0];
        }
    }
}
