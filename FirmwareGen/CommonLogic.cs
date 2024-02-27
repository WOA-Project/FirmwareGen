using FirmwareGen.GPT;
using FirmwareGen.VirtualDisks;
using System.IO;

namespace FirmwareGen
{
    internal class CommonLogic
    {
        public static byte[] GetPrimaryGPT(IDeviceProfile deviceProfile)
        {
            ulong DiskSize = deviceProfile.GetDiskTotalSize();
            uint SectorSize = deviceProfile.GetDiskSectorSize();

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
                .. GPTUtils.MakeGPT(DiskSize, SectorSize, deviceProfile.GetPartitionLayout(), IsBackupGPT: false, SplitInHalf: deviceProfile.GetSplittingStrategy() == SplittingStrategy.HalfSplit)
            ];
        }

        public static byte[] GetBackupGPT(IDeviceProfile deviceProfile)
        {
            ulong DiskSize = deviceProfile.GetDiskTotalSize();
            uint SectorSize = deviceProfile.GetDiskSectorSize();

            return GPTUtils.MakeGPT(DiskSize, SectorSize, deviceProfile.GetPartitionLayout(), IsBackupGPT: true, SplitInHalf: deviceProfile.GetSplittingStrategy() == SplittingStrategy.HalfSplit);
        }

        public static string GetBlankVHD(IDeviceProfile deviceProfile)
        {
            ulong DiskSize = deviceProfile.GetDiskTotalSize();
            uint SectorSize = deviceProfile.GetDiskSectorSize();

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