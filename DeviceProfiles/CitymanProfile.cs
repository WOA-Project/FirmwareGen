namespace FirmwareGen.DeviceProfiles
{
    class CitymanProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\950XL_Broad_Availability.bin";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $"/Driver:\"{DriverFolder}\\DEVICE.INPUT.SYNAPTICS_RMI4\" " +
                    $"/Driver:\"{DriverFolder}\\DEVICE.SOC_QC8994.CITYMAN\" " +
                    $"/Driver:\"{DriverFolder}\\DEVICE.USB.MMO_USBC\" " +
                    $"/Driver:\"{DriverFolder}\\OEM.SOC_QC8994.MMO\" " +
                    $"/Driver:\"{DriverFolder}\\OEM.SOC_QC8994.MMO_SOC8994\" " +
                    $"/Driver:\"{DriverFolder}\\PLATFORM.SOC_QC8994.BASE\" " +
                    $"/Driver:\"{DriverFolder}\\PLATFORM.SOC_QC8994.MMO\" " +
                    $"/Driver:\"{DriverFolder}\\PLATFORM.SOC_QC8994.SOC8994\" " +
                    $"/Driver:\"{DriverFolder}\\PLATFORM.SOC_QC8994.SOC8994AB\" " +
                    $"/Driver:\"{DriverFolder}\\SUPPORT.DESKTOP.BASE\" " +
                    $"/Driver:\"{DriverFolder}\\SUPPORT.DESKTOP.EXTRAS\" " +
                    $"/Driver:\"{DriverFolder}\\SUPPORT.DESKTOP.MMO_EXTRAS\" " +
                    $"/Driver:\"{DriverFolder}\\SUPPORT.DESKTOP.MOBILE_BRIDGE\" " +
                    $"/Driver:\"{DriverFolder}\\SUPPORT.DESKTOP.MOBILE_COMPONENTS\" /Recurse";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"Cityman_{OSVersion}_CLIENT{Sku}_ARM64FRE_{Language}.ffu";
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
