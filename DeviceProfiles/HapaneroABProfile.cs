using System.IO;
using System.Linq;

namespace FirmwareGen.DeviceProfiles
{
    internal class HapaneroABProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\RX130_MSM8994AB.bin";
        }

        public string UEFIELFPath()
        {
            return @"bin\RX130.elf";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\rx130ab.txt";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"RX130v2_1078.0053.1067.0000.{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8994.P6170";
        }

        public string[] SupplementaryBCDCommands()
        {
            return System.Array.Empty<string>();
        }
    }
}
