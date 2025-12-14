using FirmwareGen.CommandLine;
using FirmwareGen.DeviceProfiles;
using FirmwareGen.Streams;
using System;
using System.IO;
using System.Linq;

namespace FirmwareGen
{
    public static class MainLogic
    {
        private static readonly IDeviceProfile[] deviceProfiles =
        [
            new CitymanProfile(),
            new TalkmanProfile(),
            new HapaneroABProfile()
            //new HapaneroAAProfile(),
        ];

        public static bool VerifyAllComponentsArePresent()
        {
            const string BlankVHD = "blank.vhdx";
            const string wimlib = "wimlib\\wimlib-imagex.exe";
            const string Img2Ffu = "img2ffu\\Img2Ffu.exe";
            const string DriverUpdater = "DriverUpdater\\DriverUpdater.exe";

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

            if (!File.Exists(BlankVHD))
            {
                Logging.Log($"Some components could not be found: {BlankVHD}", Logging.LoggingLevel.Error);
                return false;
            }

            if (!File.Exists(DriverUpdater))
            {
                Logging.Log($"Some components could not be found: {DriverUpdater}", Logging.LoggingLevel.Error);
                return false;
            }

            foreach (IDeviceProfile profile in deviceProfiles)
            {
                if (!File.Exists(profile.Bootloader()))
                {
                    Logging.Log($"Bootloader components could not be found: {profile.Bootloader()}", Logging.LoggingLevel.Error);
                    return false;
                }
            }

            return true;
        }

        public static void GenerateWindowsBaseVHDX(GenerateWindowsOptions options)
        {
            const string BlankVHD = "blank.vhdx";
            const string wimlib = "wimlib\\wimlib-imagex.exe";
            const string SystemPartition = "Y:";

            Logging.Log("Copying Blank Main VHD");
            VolumeUtils.CopyFile(BlankVHD, options.Output);
            string DiskId = VolumeUtils.MountVirtualHardDisk(options.Output, false);
            string VHDLetter = VolumeUtils.GetVirtualHardDiskLetterFromDiskID(DiskId);
            VolumeUtils.ApplyWindowsImageFromDVD(wimlib, options.WindowsDVD, options.WindowsIndex, VHDLetter);
            VolumeUtils.PerformSlabOptimization(VHDLetter);
            VolumeUtils.ApplyCompactFlagsToImage(VHDLetter);
            VolumeUtils.MountSystemPartition(DiskId, SystemPartition);
            VolumeUtils.ConfigureBootManager(VHDLetter, SystemPartition);
            VolumeUtils.UnmountSystemPartition(DiskId, SystemPartition);
            VolumeUtils.DismountVirtualHardDisk(options.Output);
        }

        public static void GenerateWindowsFFU(GenerateWindowsFFUOptions options)
        {
            const string tmp = "tmp";
            const string SystemPartition = "Y:";
            const string Img2Ffu = "img2ffu\\Img2Ffu.exe";
            const string DriverUpdater = "DriverUpdater\\DriverUpdater.exe";

            if (!Directory.Exists(tmp))
            {
                _ = Directory.CreateDirectory(tmp);
            }

            foreach (IDeviceProfile deviceProfile in deviceProfiles)
            {
                const string TmpVHD = $@"{tmp}\temp.vhdx";

                Logging.Log("Copying Main VHD");
                VolumeUtils.CopyFile(options.Input, TmpVHD);
                string DiskId = VolumeUtils.MountVirtualHardDisk(TmpVHD, false);
                string TVHDLetter = VolumeUtils.GetVirtualHardDiskLetterFromDiskID(DiskId);

                Logging.Log("Writing Bootloader");
                WriteBootLoaderToDisk(DiskId, deviceProfile);
                VolumeUtils.DismountVirtualHardDisk(TmpVHD);
                DiskId = VolumeUtils.MountVirtualHardDisk(TmpVHD, false);

                Logging.Log("Writing UEFI");
                File.Copy(deviceProfile.UEFIELFPath(), $@"{TVHDLetter}\EFIESP\UEFI.elf", true);

                if (deviceProfile.SupplementaryBCDCommands().Length > 0)
                {
                    VolumeUtils.MountSystemPartition(DiskId, SystemPartition);

                    Logging.Log("Configuring supplemental boot");
                    foreach (string command in deviceProfile.SupplementaryBCDCommands())
                    {
                        VolumeUtils.RunProgram("bcdedit.exe", $"{$@"/store {SystemPartition}\EFI\Microsoft\Boot\BCD "}{command}");
                    }

                    VolumeUtils.UnmountSystemPartition(DiskId, SystemPartition);
                }

                Logging.Log("Adding drivers");
                VolumeUtils.RunProgram(DriverUpdater, $"{deviceProfile.DriverCommand(options.DriverPack)} {options.DriverPack} {TVHDLetter}");

                VolumeUtils.DismountVirtualHardDisk(TmpVHD);

                Logging.Log("Making FFU");
                string version = options.WindowsVer;
                if (version.Split(".").Length == 4)
                {
                    version = string.Join(".", version.Split(".").Skip(2));
                }

                VolumeUtils.RunProgram(Img2Ffu, $@"-i {TmpVHD} -f ""{$@"{options.Output}\{deviceProfile.FFUFileName(version, "en-us", "PROFESSIONAL")}"}"" -p {deviceProfile.PlatformID()} -o {options.WindowsVer}");

                Logging.Log("Deleting Temp VHD");
                File.Delete(TmpVHD);
            }
        }

        public static void WriteBootLoaderToDisk(string DiskId, IDeviceProfile deviceProfile)
        {
            const int chunkSize = 131072;
            DeviceStream ds = new(DiskId, FileAccess.ReadWrite);
            _ = ds.Seek(0, SeekOrigin.Begin);
            byte[] bootloader = File.ReadAllBytes(deviceProfile.Bootloader());

            int sectors = bootloader.Length / chunkSize;

            DateTime startTime = DateTime.Now;
            using (BinaryReader br = new(new MemoryStream(bootloader)))
            {
                for (int i = 0; i < sectors; i++)
                {
                    byte[] buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    Logging.ShowProgress((ulong)bootloader.Length, startTime, (ulong)((i + 1) * chunkSize), (ulong)((i + 1) * chunkSize), false);
                }
            }

            Logging.Log("");

            ds.Dispose();
        }

        public static void GenerateOtherFFU(GenerateOtherFFUOptions options)
        {
            const string tmp = "tmp";
            const string Img2Ffu = "img2ffu\\Img2Ffu.exe";

            if (!Directory.Exists(tmp))
            {
                _ = Directory.CreateDirectory(tmp);
            }

            foreach (IDeviceProfile deviceProfile in deviceProfiles)
            {
                const string TmpVHD = $@"{tmp}\temp.vhdx";
                Logging.Log("Copying Main VHD");
                VolumeUtils.CopyFile(options.Input, TmpVHD);
                string DiskId = VolumeUtils.MountVirtualHardDisk(TmpVHD, false);
                _ = VolumeUtils.GetVirtualHardDiskLetterFromDiskID(DiskId);

                Logging.Log("Writing Bootloader");
                WriteBootLoaderToDisk(DiskId, deviceProfile);
                VolumeUtils.DismountVirtualHardDisk(TmpVHD);
                DiskId = VolumeUtils.MountVirtualHardDisk(TmpVHD, false);

                Logging.Log("Making FFU");
                VolumeUtils.RunProgram(Img2Ffu, $@"-i \\.\PhysicalDrive{DiskId} -f ""{options.Output + "\\" + deviceProfile.FFUFileName(options.Ver, "en-us", "PROFESSIONAL")}"" -p {deviceProfile.PlatformID()} -o {options.Ver}");
                VolumeUtils.DismountVirtualHardDisk(TmpVHD);

                Logging.Log("Deleting Temp VHD");
                File.Delete(TmpVHD);
            }
        }
    }
}
