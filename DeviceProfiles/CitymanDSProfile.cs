namespace FirmwareGen.DeviceProfiles
{
    class CitymanDSProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\950XL_Broad_Availability.bin";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $"/Driver:\"{DriverFolder}\\msm8992-8994\" " +
                $"/Driver:\"{DriverFolder}\\msm8994\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\battmngr-registry\\Cityman\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\bootloader-dual\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\specifics-mmo\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\touch-lumia950\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\soc-final\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\oeminfo-citymands\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\display-oled\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\graphics-cityman\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\specifics-cityman\" /Recurse";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"RM-1116_{OSVersion}_CLIENT{Sku}_ARM64FRE_{Language}.ffu";
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
