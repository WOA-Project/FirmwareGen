using FirmwareGen.CommandLine;
using FirmwareGen.DeviceProfiles;
using System.IO;
using System.Linq;

namespace FirmwareGen
{
    public static class MainLogic
    {
        private static readonly DeviceProfile[] deviceProfiles =
        [
            new EpsilonHalfSplit128GB(),
            new EpsilonHalfSplit256GB(),
            new EpsilonMaximizedForWindows(),
            new ZetaHalfSplit128GB(),
            new ZetaHalfSplit256GB(),
            new ZetaMaximizedForWindows()
        ];

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

            foreach (DeviceProfile deviceProfile in deviceProfiles)
            {
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
                string version = options.WindowsVer;
                if (version.Split(".").Length == 4)
                {
                    version = string.Join(".", version.Split(".").Skip(2));
                }

                VolumeUtils.RunProgram(Img2Ffu, $@"-i {TmpVHD} -f ""{options.Output}\{deviceProfile.FFUFileName}"" -c {deviceProfile.DiskSectorSize * 4} -s {deviceProfile.DiskSectorSize} -p ""{string.Join(";", deviceProfile.PlatformIDs)}"" -o {options.WindowsVer} -b 4000");

                Logging.Log("Deleting Temp VHD");
                File.Delete(TmpVHD);
            }
        }
    }
}
