using FirmwareGen.DeviceProfiles;
using FirmwareGen.Extensions;
using FirmwareGen.Streams;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static FirmwareGen.CLIOptions;

namespace FirmwareGen
{
    public class MainLogic
    {
        static IDeviceProfile[] deviceProfiles = new IDeviceProfile[]
        {
            new CitymanProfile(),
            new TalkmanProfile(),
            //new HapaneroABProfile()
            //new HapaneroAAProfile(),
        };

        public static bool VerifyAllComponentsArePresent()
        {
            var BlankVHD = @"blank.vhdx";
            var wimlib = @"wimlib-imagex.exe";
            var Img2Ffu = @"Img2Ffu.exe";

            if (!(File.Exists(wimlib) && File.Exists(Img2Ffu) && File.Exists(BlankVHD)))
            {
                Logging.Log("Some components could not be found", Logging.LoggingLevel.Error);
                return false;
            }

            foreach (var profile in deviceProfiles)
            {
                if (!File.Exists(profile.Bootloader()))
                {
                    Logging.Log("Bootloader components could not be found", Logging.LoggingLevel.Error);
                    return false;
                }
            }

            return true;
        }

        public static string MountVHD(string VHDPath, bool readOnly)
        {
            Logging.Log("Mounting " + VHDPath + (readOnly ? " as read only" : "") + "...");
            //RunProgram("powershell.exe", $"-command \"Import-module hyper-v; Mount-VHD -Path '{VHDPath}'" + (readOnly ? " -ReadOnly" : "") + "\"");
            var id = VHDUtils.MountVHD(VHDPath, readOnly);
            Logging.Log(id, Logging.LoggingLevel.Warning);
            return id;
        }

        public static void DismountVHD(string VHDPath)
        {
            Logging.Log("Dismounting " + VHDPath + "...");
            //RunProgram("powershell.exe", $"-command \"Import-module hyper-v; Dismount-VHD -Path '{VHDPath}'\"");
            VHDUtils.UnmountVHD(VHDPath);
        }

        public static string GetVHDLetter(string DiskId)
        {
            string TVHDLetter = null;
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo("powershell.exe", "-command \"(Get-Partition -DiskNumber " + DiskId + " | Where {$_.DriveLetter}).DriveLetter\"");
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                Logging.Log("VHD mounted at: " + line);
                TVHDLetter = line + ":";
            }
            proc.WaitForExit();
            return TVHDLetter;
        }

        public static void RunProgram(string Program, string Arguments)
        {
            Process proc = new Process();
            proc.StartInfo = new ProcessStartInfo(Program, Arguments);
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();
        }

        public static void CopyFile(string Source, string Dest)
        {
            var _source = new FileInfo(Source);
            var _destination = new FileInfo(Dest);

            DateTime startTime = DateTime.Now;
            _source.CopyTo(_destination, x => Logging.ShowProgress(100, startTime, (UInt64)x, (UInt64)x, false));
            Logging.Log("");
        }

        public static void GenerateWindowsBaseVHDX(GenerateWindowsOptions options)
        {
            var BlankVHD = @"blank.vhdx";
            var wimlib = @"wimlib-imagex.exe";

            var SystemPartition = "Y:";

            Logging.Log("Copying Blank Main VHD");
            CopyFile(BlankVHD, options.Output);

            string DiskId = MountVHD(options.Output, false);

            var VHDLetter = GetVHDLetter(DiskId);

            Logging.Log("Applying image");
            RunProgram(wimlib, $"apply {options.WindowsDVD}\\sources\\install.wim {options.WindowsIndex} {VHDLetter} --compact=LZX");

            Logging.Log("Slab optimization");
            RunProgram("C:\\Windows\\System32\\defrag.exe", $"{VHDLetter} /K /X");

            Logging.Log("Applying compact flags");
            RunProgram("reg.exe", $"load HKLM\\RTSYSTEM {VHDLetter}\\Windows\\System32\\config\\SYSTEM");
            RunProgram("reg.exe", @"add HKLM\RTSYSTEM\Setup /v Compact /t REG_DWORD /d 1");
            RunProgram("reg.exe", @"unload HKLM\RTSYSTEM");
            RunProgram("reg.exe", $"load HKLM\\RTSOFTWARE {VHDLetter}\\Windows\\System32\\config\\SOFTWARE");
            RunProgram("reg.exe", "add \"HKLM\\RTSOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Wof\" /v ForceAlgorithm /t REG_DWORD /d 1");
            RunProgram("reg.exe", @"unload HKLM\RTSOFTWARE");

            Logging.Log("Mounting SYSTEM");
            RunProgram("powershell.exe", "-command \"Get-Partition -DiskNumber " + DiskId + " | Where {$_.Type -eq 'System'} | Set-Partition -NewDriveLetter '" + SystemPartition + "'.Substring(0,1)\"");

            Logging.Log("Configuring boot");
            RunProgram("bcdboot.exe", $"{VHDLetter}\\Windows /s {SystemPartition} /f UEFI /l en-us");

            RunProgram("bcdedit.exe", $"/store {SystemPartition}\\EFI\\Microsoft\\Boot\\BCD " + "/set {default} testsigning on");
            RunProgram("bcdedit.exe", $"/store {SystemPartition}\\EFI\\Microsoft\\Boot\\BCD " + "/set {default} nointegritychecks on");

            Logging.Log("Unmounting SYSTEM");
            RunProgram("powershell.exe", "-command \"Get-Partition -DiskNumber " + DiskId + " | Where {$_.Type -eq 'System'} | Remove-PartitionAccessPath -accesspath '" + SystemPartition + "'\"");

            DismountVHD(options.Output);
        }

        public static void GenerateWindowsFFU(GenerateWindowsFFUOptions options)
        {
            var tmp = @"tmp";
            var SystemPartition = "Y:";
            var Img2Ffu = @"Img2Ffu.exe";

            if (!Directory.Exists(tmp))
                Directory.CreateDirectory(tmp);

            foreach (var deviceProfile in deviceProfiles)
            {
                var TmpVHD = tmp + @"\temp.vhdx";
                Logging.Log("Copying Main VHD");
                CopyFile(options.Input, TmpVHD);

                string DiskId = MountVHD(TmpVHD, false);

                var TVHDLetter = GetVHDLetter(DiskId);

                Logging.Log("Writing Bootloader");
                WriteBootLoaderToDisk(DiskId, deviceProfile);

                DismountVHD(TmpVHD);

                DiskId = MountVHD(TmpVHD, false);

                if (deviceProfile.SupplementaryBCDCommands().Count() > 0)
                {
                    Logging.Log("Mounting SYSTEM");
                    RunProgram("powershell.exe", "-command \"Get-Partition -DiskNumber " + DiskId + " | Where {$_.Type -eq 'System'} | Set-Partition -NewDriveLetter '" + SystemPartition + "'.Substring(0,1)\"");

                    Logging.Log("Configuring supplemental boot");
                    foreach (var command in deviceProfile.SupplementaryBCDCommands())
                    {
                        RunProgram("bcdedit.exe", $"/store {SystemPartition}\\EFI\\Microsoft\\Boot\\BCD " + command);
                    }

                    Logging.Log("Unmounting SYSTEM");
                    RunProgram("powershell.exe", "-command \"Get-Partition -DiskNumber " + DiskId + " | Where {$_.Type -eq 'System'} | Remove-PartitionAccessPath -accesspath '" + SystemPartition + "'\"");
                }

                Logging.Log("Adding drivers");
                RunProgram("dism.exe", $"/Image:{TVHDLetter} /Add-Driver " + deviceProfile.DriverCommand(options.DriverPack));

                DismountVHD(TmpVHD);

                DiskId = MountVHD(TmpVHD, true);

                Logging.Log("Making FFU");
                RunProgram(Img2Ffu, $"-i \\\\.\\PhysicalDrive{DiskId} -f \"{options.Output + "\\" + deviceProfile.FFUFileName(options.WindowsVer, "EN-US", "PRO")}\" -p {deviceProfile.PlatformID()} -o {options.WindowsVer}");

                DismountVHD(TmpVHD);

                Logging.Log("Deleting Temp VHD");
                File.Delete(TmpVHD);
            }
        }

        public static void WriteBootLoaderToDisk(string DiskId, IDeviceProfile deviceProfile)
        {
            var chunkSize = 131072;
            DeviceStream ds = new DeviceStream(DiskId, FileAccess.ReadWrite);
            ds.Seek(0, SeekOrigin.Begin);
            byte[] bootloader = File.ReadAllBytes(deviceProfile.Bootloader());

            var sectors = bootloader.Count() / chunkSize;

            DateTime startTime = DateTime.Now;
            using (BinaryReader br = new BinaryReader(new MemoryStream(bootloader)))
                for (int i = 0; i < sectors; i++)
                {
                    var buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    Logging.ShowProgress((UInt64)bootloader.Count(), startTime, (UInt64)((i + 1) * chunkSize), (UInt64)((i + 1) * chunkSize), false);
                }
            Logging.Log("");

            ds.Dispose();
        }

        public static void GenerateOtherFFU(GenerateOtherFFUOptions options)
        {
            var tmp = @"tmp";
            var Img2Ffu = @"Img2Ffu.exe";

            if (!Directory.Exists(tmp))
                Directory.CreateDirectory(tmp);

            foreach (var deviceProfile in deviceProfiles)
            {
                var TmpVHD = tmp + @"\temp.vhdx";
                Logging.Log("Copying Main VHD");
                CopyFile(options.Input, TmpVHD);

                string DiskId = MountVHD(TmpVHD, false);

                var TVHDLetter = GetVHDLetter(DiskId);

                Logging.Log("Writing Bootloader");
                WriteBootLoaderToDisk(DiskId, deviceProfile);

                DismountVHD(TmpVHD);

                DiskId = MountVHD(TmpVHD, true);

                Logging.Log("Making FFU");
                RunProgram(Img2Ffu, $"-i \\\\.\\PhysicalDrive{DiskId} -f \"{options.Output + "\\" + deviceProfile.FFUFileName(options.Ver, "EN-US", "PRO")}\" -p {deviceProfile.PlatformID()} -o {options.Ver}");

                DismountVHD(TmpVHD);

                Logging.Log("Deleting Temp VHD");
                File.Delete(TmpVHD);
            }
        }
    }
}
