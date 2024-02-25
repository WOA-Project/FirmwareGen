using FirmwareGen.GPT;
using FirmwareGen.VirtualDisks;
using System.IO;

namespace FirmwareGen.DeviceProfiles
{
    internal class MTP8150MaximizedForWindows : IDeviceProfile
    {
        public byte[] GetPrimaryGPT()
        {
            ulong DiskSize = 123_371_257_856; // 128GB;
            ulong SectorSize = 4096;

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
                .. GPTUtils.MakeGPT(DiskSize, SectorSize, Constants.MTP8150_UFS_LUN_0_PARTITIONS, IsBackupGPT: false, SplitInHalf: false)
            ];
        }

        public byte[] GetBackupGPT()
        {
            ulong DiskSize = 123_371_257_856; // 128GB;
            ulong SectorSize = 4096;

            return GPTUtils.MakeGPT(DiskSize, SectorSize, Constants.MTP8150_UFS_LUN_0_PARTITIONS, IsBackupGPT: true, SplitInHalf: false);
        }

        public string GetBlankVHD()
        {
            ulong DiskSize = 123_371_257_856; // 128GB;
            uint SectorSize = 4096;

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
            byte[] PrimaryGPT = GetPrimaryGPT();

            Logging.Log("Generating Backup GPT");
            byte[] BackupGPT = GetBackupGPT();

            Logging.Log("Generating Main VHD");
            VHDUtils.CreateVHDX(TmpVHD, SectorSize, DiskSize);

            BlankVHDUtils.PrepareVHD(TmpVHD, PrimaryGPT, BackupGPT);

            return TmpVHD;
        }

        public string[] SupplementaryBCDCommands()
        {
            return [];
        }

        public string PlatformID()
        {
            return "Qualcomm.Surface.MTP.SM8150";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"QCOM_MTP_8150_MaximizedForWindows_{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\mtp855.xml";
        }
    }
}
