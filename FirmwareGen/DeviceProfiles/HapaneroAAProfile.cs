namespace FirmwareGen.DeviceProfiles
{
    internal class HapaneroAAProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\RX130_MSM8994AA.bin";
        }

        public string UEFIELFPath()
        {
            return @"bin\RX130AA.elf";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\rx130aa.txt";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"RX130v1_1078.0053.1067.0000.{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string PlatformID()
        {
            return "Qualcomm.MSM8994.P6170";
        }

        public string[] SupplementaryBCDCommands()
        {
            return [
                "/set {default} numproc 4",
                "/set {bootmgr} processcustomactionsfirst Yes",
                "/set {bootmgr} customactions 0x1000048000001 0x54000001 0x1000050000001 0x54000002 0x10000000d0001 0x54000003 0x1000000050001 0x54000003"
            ];
        }
    }
}
