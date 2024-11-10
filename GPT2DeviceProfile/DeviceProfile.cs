using FirmwareGen.GPT;
using System;

namespace FirmwareGen
{
    public class DeviceProfile
    {
        public virtual string[] SupplementaryBCDCommands
        {
            get; set;
        }
        public virtual string[] PlatformIDs
        {
            get; set;
        }
        public virtual string FFUFileName
        {
            get; set;
        }
        public virtual string DriverDefinitionPath
        {
            get; set;
        }
        public virtual ulong DiskTotalSize
        {
            get; set;
        }
        public virtual uint DiskSectorSize
        {
            get; set;
        }
        public virtual GPTPartition[] PartitionLayout
        {
            get; set;
        }
        public virtual SplittingStrategy SplittingStrategy
        {
            get; set;
        }
        public virtual ulong CustomSplittingAndroidDesiredSpace
        {
            get; set;
        }
        public virtual Guid DiskGuid
        {
            get; set;
        }
    }
}