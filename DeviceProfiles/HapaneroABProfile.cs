using System.IO;
using System.Linq;

namespace FirmwareGen.DeviceProfiles
{
    class HapaneroABProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\RX130_MSM8994AB.bin";
        }

        public string DriverCommand(string DriverFolder)
        {
            string definitionFile = $@"{DriverFolder}\definitions\rx130ab.txt";
            string[] definitionPaths = File.ReadAllLines(definitionFile).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string driverCommand = $"{string.Join(" ", definitionPaths.Select(x => $"/Driver:\"{DriverFolder}\\{x}\""))} /Recurse";
            return driverCommand;
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"{OSVersion}_CLIENT{Sku}_HAPANEROV2_A64FRE_{Language}.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8994.P6170";
        }

        public string[] SupplementaryBCDCommands()
        {
            return new string[0];
        }
    }
}
