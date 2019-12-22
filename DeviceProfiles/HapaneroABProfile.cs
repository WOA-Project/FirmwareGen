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
            return $"/Driver:\"{DriverFolder}\\msm8992-8994\" " +
                $"/Driver:\"{DriverFolder}\\msm8994\" " +
                $"/Driver:\"{DriverFolder}\\support-desktop\" " +
                $"/Driver:\"{DriverFolder}\\support-aarch64\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\oems\\specifics-mmo\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\soc-final\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\devices\\specifics-hapanero\" /Recurse";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"HapaneroV2_{OSVersion}_CLIENT{Sku}_ARM64FRE_{Language}.ffu";
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
