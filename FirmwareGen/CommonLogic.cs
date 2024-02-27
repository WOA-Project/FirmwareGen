using FirmwareGen.GPT;
using FirmwareGen.VirtualDisks;
using System.IO;

namespace FirmwareGen
{
    internal class CommonLogic
    {
        public static byte[] GetPrimaryGPT(DeviceProfile deviceProfile)
        {
            ulong DiskSize = deviceProfile.DiskTotalSize;
            uint SectorSize = deviceProfile.DiskSectorSize;

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
                .. GPTUtils.MakeGPT(DiskSize, SectorSize, deviceProfile.PartitionLayout, deviceProfile.DiskGuid, IsBackupGPT: false, SplitInHalf: deviceProfile.SplittingStrategy == SplittingStrategy.HalfSplit, AndroidDesiredSpace: deviceProfile.SplittingStrategy == SplittingStrategy.Custom ? deviceProfile.CustomSplittingAndroidDesiredSpace : 4_294_967_296)
            ];
        }

        public static byte[] GetBackupGPT(DeviceProfile deviceProfile)
        {
            ulong DiskSize = deviceProfile.DiskTotalSize;
            uint SectorSize = deviceProfile.DiskSectorSize;

            return GPTUtils.MakeGPT(DiskSize, SectorSize, deviceProfile.PartitionLayout, deviceProfile.DiskGuid, IsBackupGPT: true, SplitInHalf: deviceProfile.SplittingStrategy == SplittingStrategy.HalfSplit, AndroidDesiredSpace: deviceProfile.SplittingStrategy == SplittingStrategy.Custom ? deviceProfile.CustomSplittingAndroidDesiredSpace : 4_294_967_296);
        }

        public static string GetBlankVHD(DeviceProfile deviceProfile)
        {
            ulong DiskSize = deviceProfile.DiskTotalSize;
            uint SectorSize = deviceProfile.DiskSectorSize;

            const string tmp = "tmp";
            const string TmpVHD = $@"{tmp}\temp.vhdx";

            if (!Directory.Exists(tmp))
            {
                _ = Directory.CreateDirectory(tmp);
            }

            if (File.Exists(TmpVHD))
            {
                File.Delete(TmpVHD);
            }

            Logging.Log("Generating Primary GPT");
            byte[] PrimaryGPT = GetPrimaryGPT(deviceProfile);

            Logging.Log("Generating Backup GPT");
            byte[] BackupGPT = GetBackupGPT(deviceProfile);

            Logging.Log("Generating Main VHD");
            VHDUtils.CreateVHDX(TmpVHD, SectorSize, DiskSize);

            BlankVHDUtils.PrepareVHD(TmpVHD, PrimaryGPT, BackupGPT);

            return TmpVHD;
        }
    }
}