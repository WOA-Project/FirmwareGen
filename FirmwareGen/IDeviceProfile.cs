using FirmwareGen.GPT;
using System;

namespace FirmwareGen
{
    public interface IDeviceProfile
    {
        string[] GetSupplementaryBCDCommands();
        string[] GetPlatformIDs();
        string GetFFUFileName();
        string GetDriverDefinitionPath();
        ulong GetDiskTotalSize();
        uint GetDiskSectorSize();
        GPTPartition[] GetPartitionLayout();
        SplittingStrategy GetSplittingStrategy();
        ulong GetCustomSplittingAndroidDesiredSpace();
        Guid GetDiskGuid();
    }
}