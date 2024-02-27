using FirmwareGen.GPT;

namespace FirmwareGen
{
    public interface IDeviceProfile
    {
        string[] GetSupplementaryBCDCommands();
        string[] GetPlatformIDs();
        string GetFFUFileName(string OSVersion, string Language, string Sku);
        string GetDriverDefinitionPath(string DriverFolder);
        ulong GetDiskTotalSize();
        uint GetDiskSectorSize();
        GPTPartition[] GetPartitionLayout();
        SplittingStrategy GetSplittingStrategy();
    }
}