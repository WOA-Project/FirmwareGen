using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FirmwareGen.DeviceProfiles;
using FirmwareGen.Streams;
using CommandLine;
using FirmwareGen.Extensions;

namespace FirmwareGen
{
    class Program
    {
        static IDeviceProfile[] deviceProfiles = new IDeviceProfile[]
        {
            new CitymanProfile(),
            new TalkmanProfile(),
            new HapaneroABProfile()
            //new HapaneroAAProfile(),
        };

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                Logging.Log("FirmwareGen");
                Logging.Log("Copyright (c) 2019-2020, Gustave Monce - gus33000.me - @gus33000");
                Logging.Log("Released under the MIT license at github.com/gus33000/FirmwareGen");
                Logging.Log("");

                try
                {
                    GenFirmware(o.DriverPack, o.WindowsDVD, o.WindowsIndex, o.WindowsVer, o.Output, o.UseExistingVhd, o.DeleteVhd, !o.IsNotWindows);
                }
                catch (Exception ex)
                {
                    Logging.Log("Something happened.", Logging.LoggingLevel.Error);
                    Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                    Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                    Environment.Exit(1);
                }
            });
        }
        public static void GenFirmware(string DriverPackLocation, string WindowsDVD, string WindowsIndex, string OSBuild, string output, bool UseExistingVhd, bool DeleteVhd, bool IsWindows)
        {
            var BlankVHD = @"blank.vhdx";
            var tmp = @"tmp";

            if (!IsWindows)
                UseExistingVhd = true;

            if (UseExistingVhd)
                DeleteVhd = false;

            if (!Directory.Exists(tmp))
                Directory.CreateDirectory(tmp);

            var wimlib = @"wimlib-imagex.exe";
            var Img2Ffu = @"Img2Ffu.exe";

            if (!(File.Exists(wimlib) && File.Exists(Img2Ffu)))
            {
                Logging.Log("Some components could not be found", Logging.LoggingLevel.Error);
                return;
            }

            foreach (var profile in deviceProfiles)
            {
                if (!File.Exists(profile.Bootloader()))
                {
                    Logging.Log("Bootloader components could not be found", Logging.LoggingLevel.Error);
                    return;
                }
            }

            if (!File.Exists(BlankVHD))
            {
                Logging.Log("Base blank vhdx could not be found", Logging.LoggingLevel.Error);
                return;
            }

            var SystemPartition = "Y:";

            var MainVHD = tmp + @"\main.vhdx";

            if (!UseExistingVhd)
            {
                Logging.Log("Copying Blank Main VHD");
                CopyFile(BlankVHD, MainVHD);

                string DiskId = MountVHD(MainVHD, false);

                var VHDLetter = GetVHDLetter(DiskId);

                Logging.Log("Applying image");
                RunProgram(wimlib, $"apply {WindowsDVD}\\sources\\install.wim {WindowsIndex} {VHDLetter} --compact=LZX");

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

                DismountVHD(MainVHD);
            }

            foreach (var deviceProfile in deviceProfiles)
            {
                var TmpVHD = tmp + @"\temp.vhdx";
                Logging.Log("Copying Main VHD");
                CopyFile(MainVHD, TmpVHD);

                string DiskId = MountVHD(TmpVHD, false);

                var TVHDLetter = GetVHDLetter(DiskId);

                Logging.Log("Writing Bootloader");
                WriteBootLoaderToDisk(DiskId, deviceProfile);

                DismountVHD(TmpVHD);

                if (IsWindows)
                {
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
                    RunProgram("dism.exe", $"/Image:{TVHDLetter} /Add-Driver " + deviceProfile.DriverCommand(DriverPackLocation));

                    DismountVHD(TmpVHD);
                }

                DiskId = MountVHD(TmpVHD, true);

                Logging.Log("Making FFU");
                RunProgram(Img2Ffu, $"-i \\\\.\\PhysicalDrive{DiskId} -f \"{output + "\\" + deviceProfile.FFUFileName(OSBuild, "EN-US", "PRO")}\" -p {deviceProfile.PlatformID()} -o {OSBuild}");

                DismountVHD(TmpVHD);

                Logging.Log("Deleting Temp VHD");
                File.Delete(TmpVHD);
            }

            if (DeleteVhd)
            {
                Logging.Log("Deleting Main VHD");
                File.Delete(MainVHD);
            }
        }

        private static void ShowProgress(ulong totalBytes, DateTime startTime, ulong BytesRead, ulong SourcePosition, bool DisplayRed)
        {
            var now = DateTime.Now;
            var timeSoFar = now - startTime;

            var remaining = TimeSpan.FromMilliseconds(timeSoFar.TotalMilliseconds / BytesRead * (totalBytes - BytesRead));

            var speed = Math.Round(SourcePosition / 1024L / 1024L / timeSoFar.TotalSeconds);

            Logging.Log(string.Format("{0} {1}MB/s {2:hh\\:mm\\:ss\\.f}", GetDismLikeProgBar(int.Parse((BytesRead * 100 / totalBytes).ToString())), speed.ToString(), remaining, remaining.TotalHours, remaining.Minutes, remaining.Seconds, remaining.Milliseconds), returnline: false, severity: DisplayRed ? Logging.LoggingLevel.Warning : Logging.LoggingLevel.Information);
        }

        private static string GetDismLikeProgBar(int perc)
        {
            var eqsLength = (int)((double)perc / 100 * 55);
            var bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
            bases = bases.Insert(28, perc + "%");
            if (perc == 100)
                bases = bases.Substring(1);
            else if (perc < 10)
                bases = bases.Insert(28, " ");
            return "[" + bases + "]";
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
                    ShowProgress((UInt64)bootloader.Count(), startTime, (UInt64)((i + 1) * chunkSize), (UInt64)((i + 1) * chunkSize), false);
                }
            Logging.Log("");

            ds.Dispose();
        }

        public static void CopyFile(string Source, string Dest)
        {
            var _source = new FileInfo(Source);
            var _destination = new FileInfo(Dest);

            DateTime startTime = DateTime.Now;
            _source.CopyTo(_destination, x => ShowProgress(100, startTime, (UInt64)x, (UInt64)x, false));
            Logging.Log("");
        }

        internal class Options
        {
            [Option('d', "driver-pack", HelpText = @"Todo", Required = true)]
            public string DriverPack { get; set; }

            [Option('o', "output", HelpText = "Todo", Required = true)]
            public string Output { get; set; }

            /*[Option('m', "mount-id", HelpText = "Todo", Required = true)]
            public string DiskId { get; set; }*/

            [Option('u', "use-existingvhd", HelpText = "Use the existing vhd without deleting it", Required = false, Default = false)]
            public bool UseExistingVhd { get; set; }

            [Option('c', "clean-vhd", HelpText = "Clean the vhd once done", Required = false, Default = false)]
            public bool DeleteVhd { get; set; }

            [Option('w', "windows-dvd", HelpText = "Todo", Required = true)]
            public string WindowsDVD { get; set; }

            [Option('z', "is-notwindows", HelpText = "Todo", Required = false, Default = false)]
            public bool IsNotWindows { get; set; }

            [Option('i', "windows-index", HelpText = "Todo", Required = true)]
            public string WindowsIndex { get; set; }

            [Option('v', "windows-ver", HelpText = "Todo", Required = true)]
            public string WindowsVer { get; set; }
        }
    }
}
