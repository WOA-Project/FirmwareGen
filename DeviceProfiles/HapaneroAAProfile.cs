using System.IO;
using System.Linq;

namespace FirmwareGen.DeviceProfiles
{
    class HapaneroAAProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\RX130_MSM8994AA.bin";
        }

        public string DriverCommand(string DriverFolder)
        {
            string definitionFile = $@"{DriverFolder}\definitions\rx130aa.txt";
            string[] definitionPaths = File.ReadAllLines(definitionFile).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string driverCommand = $"{string.Join(" ", definitionPaths.Select(x => $"/Driver:\"{DriverFolder}\\{x}\""))} /Recurse";
            return driverCommand;
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"{OSVersion}_CLIENT{Sku}_HAPANEROV1_A64FRE_{Language}.ffu";
        }

        public string PlatformID()
        {
            return "Qualcomm.MSM8994.P6170";
        }

        public string[] SupplementaryBCDCommands()
        {
            return new string[3]
            {
                "/set {default} numproc 4",
                "/set {bootmgr} processcustomactionsfirst Yes",
                "/set {bootmgr} customactions 0x1000048000001 0x54000001 0x1000050000001 0x54000002 0x10000000d0001 0x54000003 0x1000000050001 0x54000003"
            };
        }
    }
}
