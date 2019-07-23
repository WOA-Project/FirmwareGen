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
            return $"/Driver:\"{DriverFolder}\\msm8992-8994\" " +
                $"/Driver:\"{DriverFolder}\\msm8994\" " +
                $"/Driver:\"{DriverFolder}\\support-desktop\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\battmngr-registry\\Hapanero\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\bootloader-single\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\specifics-mmo\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\soc-prerelease\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\graphics-hapanero\" " +
                $"/Driver:\"{DriverFolder}\\configurations\\specifics-hapanero\" /Recurse";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"HapaneroV1_{OSVersion}_CLIENT{Sku}_ARM64FRE_{Language}.ffu";
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
