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
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\950.xml";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"RM1104_1078.0053.1067.0000.{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8992.P6218";
        }

        public string[] SupplementaryBCDCommands()
        {
            return [];
        }
    }
}
