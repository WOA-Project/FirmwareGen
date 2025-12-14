namespace FirmwareGen.DeviceProfiles
{
    internal class CitymanProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\950XL_Broad_Availability.bin";
        }

        public string UEFIELFPath()
        {
            return @"bin\950XL.elf";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\950xl.xml";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"RM1085_1078.0053.1067.0000.{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8994.P6211";
        }

        public string[] SupplementaryBCDCommands()
        {
            return [];
        }
    }
}
