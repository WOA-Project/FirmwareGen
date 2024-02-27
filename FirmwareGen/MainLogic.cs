using FirmwareGen.CommandLine;
using System.IO;

namespace FirmwareGen
{
    public static class MainLogic
    {
        public static bool VerifyAllComponentsArePresent()
        {
            const string wimlib = "wimlib-imagex.exe";
            const string Img2Ffu = "Img2Ffu.exe";
            const string DriverUpdater = "DriverUpdater.exe";

            if (!File.Exists(wimlib))
            {
                Logging.Log($"Some components could not be found: {wimlib}", Logging.LoggingLevel.Error);
                return false;
            }

            if (!File.Exists(Img2Ffu))
            {
                Logging.Log($"Some components could not be found: {Img2Ffu}", Logging.LoggingLevel.Error);
                return false;
            }

            if (!File.Exists(DriverUpdater))
            {
                Logging.Log($"Some components could not be found: {DriverUpdater}", Logging.LoggingLevel.Error);
                return false;
            }

            return true;
        }

        public static void GenerateWindowsFFU(GenerateWindowsFFUOptions options)
        {
            const string wimlib = "wimlib-imagex.exe";
            const string Img2Ffu = "Img2Ffu.exe";
            const string DriverUpdater = "DriverUpdater.exe";
            const string SystemPartition = "Y:";

            DeviceProfile deviceProfile = XmlUtils.Deserialize<DeviceProfile>(options.DeviceProfile);

            string TmpVHD = CommonLogic.GetBlankVHD(deviceProfile);
            string DiskId = VolumeUtils.MountVirtualHardDisk(TmpVHD, false);
            string VHDLetter = VolumeUtils.GetVirtualHardDiskLetterFromDiskID(DiskId);

            VolumeUtils.ApplyWindowsImageFromDVD(wimlib, options.WindowsDVD, options.WindowsIndex, VHDLetter);
            VolumeUtils.PerformSlabOptimization(VHDLetter);
            VolumeUtils.ApplyCompactFlagsToImage(VHDLetter);
            VolumeUtils.MountSystemPartition(DiskId, SystemPartition);
            VolumeUtils.ConfigureBootManager(VHDLetter, SystemPartition);
            VolumeUtils.UnmountSystemPartition(DiskId, SystemPartition);

            if (deviceProfile.SupplementaryBCDCommands.Length > 0)
            {
                VolumeUtils.MountSystemPartition(DiskId, SystemPartition);

                Logging.Log("Configuring supplemental boot");
                foreach (string command in deviceProfile.SupplementaryBCDCommands)
                {
                    VolumeUtils.RunProgram("bcdedit.exe", $"{$@"/store {SystemPartition}\EFI\Microsoft\Boot\BCD "}{command}");
                }

                VolumeUtils.UnmountSystemPartition(DiskId, SystemPartition);
            }

            Logging.Log("Adding drivers");
            VolumeUtils.RunProgram(DriverUpdater, $@"-d ""{options.DriverPack}{deviceProfile.DriverDefinitionPath}"" -r ""{options.DriverPack}"" -p ""{VHDLetter}""");

            VolumeUtils.DismountVirtualHardDisk(TmpVHD);

            Logging.Log("Making FFU");
            VolumeUtils.RunProgram(Img2Ffu, $@"-i {TmpVHD} -f ""{options.Output}\{deviceProfile.FFUFileName}"" -c {deviceProfile.DiskSectorSize * 4} -s {deviceProfile.DiskSectorSize} -p ""{string.Join(";", deviceProfile.PlatformIDs)}"" -o {options.WindowsVer} -b 4000");

            Logging.Log("Deleting Temp VHD");
            File.Delete(TmpVHD);
        }
    }
}
