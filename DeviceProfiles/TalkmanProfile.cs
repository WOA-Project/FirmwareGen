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
            return $"/Driver:\"{DriverFolder}\\msm8992-8994\" " +
                $"/Driver:\"{DriverFolder}\\msm8992\" " +
                $"/Driver:\"{DriverFolder}\\support-desktop\" " +
                $"/Driver:\"{DriverFolder}\\support-aarch64\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\oems\\specifics-mmo\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\soc-final\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\devices\\specifics-talkman\" /Recurse";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"Talkman_{OSVersion}_CLIENT{Sku}_ARM64FRE_{Language}.ffu";
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
