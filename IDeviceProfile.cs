namespace FirmwareGen
{
    public interface IDeviceProfile
    {
        string Bootloader();
        string[] SupplementaryBCDCommands();
        string PlatformID();
        string FFUFileName(string OSVersion, string Language, string Sku);
        string DriverCommand(string DriverFolder);
        string UEFIELFPath();
    }
}
