using GPTFixer.FirmwareGen.GPT;
using System;

namespace GPTFixer.FirmwareGen
{
    internal static class CommonLogic
    {
        public static byte[] GetPrimaryGPT(ulong DiskTotalSize, uint DiskSectorSize, Guid DiskGuid, GPTPartition[] PartitionLayout)
        {
            ulong DiskSize = DiskTotalSize;
            uint SectorSize = DiskSectorSize;

            byte[] PrimaryMBR = new byte[SectorSize];
            PrimaryMBR[0x1C0] = 0x01;
            PrimaryMBR[0x1C2] = 0xEE;
            PrimaryMBR[0x1C3] = 0xFF;
            PrimaryMBR[0x1C4] = 0xFF;
            PrimaryMBR[0x1C5] = 0xFF;
            PrimaryMBR[0x1C6] = 0x01;
            PrimaryMBR[0x1CA] = 0xFF;
            PrimaryMBR[0x1CB] = 0xFF;
            PrimaryMBR[0x1CC] = 0xFF;
            PrimaryMBR[0x1CD] = 0xFF;
            PrimaryMBR[0x1FE] = 0x55;
            PrimaryMBR[0x1FF] = 0xAA;

            return [
                .. PrimaryMBR,
                .. GPTUtils.MakeGPT(DiskSize, SectorSize, PartitionLayout, DiskGuid, IsBackupGPT: false)
            ];
        }

        public static byte[] GetBackupGPT(ulong DiskTotalSize, uint DiskSectorSize, Guid DiskGuid, GPTPartition[] PartitionLayout)
        {
            ulong DiskSize = DiskTotalSize;
            uint SectorSize = DiskSectorSize;

            return GPTUtils.MakeGPT(DiskSize, SectorSize, PartitionLayout, DiskGuid, IsBackupGPT: true);
        }
    }
}
