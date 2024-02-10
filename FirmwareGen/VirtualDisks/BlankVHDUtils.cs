using DiscUtils.Vhdx;
using System;
using System.IO;

namespace FirmwareGen.VirtualDisks
{
    internal class BlankVHDUtils
    {
        private static void WriteGPTToDisk(string TmpVHD, byte[] PrimaryGPT, byte[] BackupGPT)
        {
            const int chunkSize = 4096;

            DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(Disk).Assembly);
            using DiscUtils.VirtualDisk outDisk = DiscUtils.VirtualDisk.OpenDisk(TmpVHD, FileAccess.ReadWrite);

            DiscUtils.Streams.SparseStream ds = outDisk.Content;

            // Primary GPT
            Logging.Log("Writing Primary GPT");
            _ = ds.Seek(0, SeekOrigin.Begin);
            int sectors = PrimaryGPT.Length / chunkSize;
            DateTime startTime = DateTime.Now;
            using (BinaryReader br = new(new MemoryStream(PrimaryGPT)))
            {
                for (int i = 0; i < sectors; i++)
                {
                    byte[] buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    Logging.ShowProgress((ulong)PrimaryGPT.Length, startTime, (ulong)((i + 1) * chunkSize), (ulong)((i + 1) * chunkSize), false);
                }
            }
            Logging.Log("");

            // Backup GPT
            Logging.Log("Writing Backup GPT");
            _ = ds.Seek(-BackupGPT.Length, SeekOrigin.End);
            sectors = BackupGPT.Length / chunkSize;
            startTime = DateTime.Now;
            using (BinaryReader br = new(new MemoryStream(BackupGPT)))
            {
                for (int i = 0; i < sectors; i++)
                {
                    byte[] buff = br.ReadBytes(chunkSize);
                    ds.Write(buff, 0, chunkSize);
                    Logging.ShowProgress((ulong)BackupGPT.Length, startTime, (ulong)((i + 1) * chunkSize), (ulong)((i + 1) * chunkSize), false);
                }
            }
            Logging.Log("");

            ds.Dispose();
        }

        public static void PrepareVHD(string TmpVHD, byte[] PrimaryGPT, byte[] BackupGPT)
        {
            const string SystemPartition = "Y:";

            Logging.Log("Writing GPT");
            WriteGPTToDisk(TmpVHD, PrimaryGPT, BackupGPT);

            Logging.Log("Mounting Main VHD");
            string DiskId = VolumeUtils.MountVirtualHardDisk(TmpVHD, false);

            Logging.Log("Getting Windows Partition Drive Letter");
            string VHDLetter = VolumeUtils.GetVirtualHardDiskLetterFromDiskID(DiskId);

            Logging.Log($"Windows Partition: {VHDLetter}");

            // Format as NTFS
            VolumeUtils.RunProgram("cmd.exe", $"/C format {VHDLetter} /FS:NTFS /V:MainOS /Q /Y");

            Logging.Log($"Mounting System Partition: {SystemPartition}");
            VolumeUtils.MountSystemPartition(DiskId, SystemPartition);

            // Format as FAT32
            VolumeUtils.RunProgram("cmd.exe", $"/C format {SystemPartition} /FS:FAT32 /V:EFIESP /Q /Y");

            Logging.Log($"Unmounting System Partition: {SystemPartition}");
            VolumeUtils.UnmountSystemPartition(DiskId, SystemPartition);

            Logging.Log("Dismounting Main VHD");
            VolumeUtils.DismountVirtualHardDisk(TmpVHD);
        }
    }
}