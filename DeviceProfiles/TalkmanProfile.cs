using System.IO;
using System.Linq;

namespace FirmwareGen.DeviceProfiles
{
    internal class TalkmanProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\950_Broad_Availability.bin";
        }

        public string UEFIELFPath()
        {
            return @"bin\950.elf";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\950.txt";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"RM1114_1078.0053.1067.0000.{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8992.P6218";
        }

        public string[] SupplementaryBCDCommands()
        {
            return System.Array.Empty<string>();
        }
    }
}
