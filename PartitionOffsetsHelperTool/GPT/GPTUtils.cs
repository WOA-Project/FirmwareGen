using System;
using System.Collections.Generic;
using System.Linq;

namespace PartitionOffsetsHelperTool.GPT
{
    internal class GPTUtils
    {
        internal static void MakeGPT(ulong DiskSize, ulong SectorSize, GPTPartition[] DefaultPartitionTable, ulong AndroidDesiredSpace = 4_294_967_296)
        {
            ulong LastLBA = (DiskSize / SectorSize) - 1;

            ulong PartitionArrayLBACount = 4;
            ulong TotalGPTLBACount = 1 /* GPT Header */ + PartitionArrayLBACount /* Partition Table */;
            ulong LastUsableLBA = LastLBA - TotalGPTLBACount;

            List<GPTPartition> Partitions = new(DefaultPartitionTable);
            Partitions[^1].LastLBA = LastUsableLBA;

            if (AndroidDesiredSpace < 4_294_967_296)
            {
                throw new Exception("ERROR");
            }

            InjectWindowsPartitions(Partitions, SectorSize, 4, AndroidDesiredSpace);
        }

        private static void InjectWindowsPartitions(List<GPTPartition> Partitions, ulong SectorSize, ulong BlockSize, ulong AndroidDesiredSpace)
        {
            ulong FirstUsableLBA = Partitions.Last().FirstLBA;
            ulong LastUsableLBA = Partitions.Last().LastLBA;

            if (LastUsableLBA % BlockSize != 0)
            {
                LastUsableLBA -= LastUsableLBA % BlockSize;
            }

            ulong UsableLBACount = LastUsableLBA - FirstUsableLBA + 1;

            ulong SixtyFourGigaBytes = 68_719_476_736 / SectorSize;

            ulong ESPLBACount = 65525 + 1024 + 1 /* Cluster Size Limit for FAT32 */;
            if (ESPLBACount % BlockSize != 0)
            {
                ESPLBACount += BlockSize - (ESPLBACount % BlockSize);
            }

            /* Strategy to reserve 4GB for Android Only */
            ulong FourGigaBytes = AndroidDesiredSpace / SectorSize;
            ulong WindowsLBACount = UsableLBACount - ESPLBACount - FourGigaBytes;

            if (WindowsLBACount < SixtyFourGigaBytes)
            {
                WindowsLBACount = SixtyFourGigaBytes;
            }

            if (WindowsLBACount % BlockSize != 0)
            {
                WindowsLBACount -= WindowsLBACount % BlockSize;
            }

            ulong TotalInjectedLBACount = ESPLBACount + WindowsLBACount;

            ulong ESPFirstLBA = LastUsableLBA - TotalInjectedLBACount;
            ulong ESPLastLBA = ESPFirstLBA + ESPLBACount - 1;

            ulong WindowsFirstLBA = ESPLastLBA + 1;
            ulong WindowsLastLBA = ESPLastLBA + WindowsLBACount;

            if (ESPFirstLBA % BlockSize != 0)
            {
                ulong Padding = BlockSize - (ESPFirstLBA % BlockSize);
                throw new Exception("ESPFirstLBA overflew block alignment by: " + Padding);
            }

            if ((ESPLastLBA + 1) % BlockSize != 0)
            {
                ulong Padding = BlockSize - ((ESPLastLBA + 1) % BlockSize);
                throw new Exception("ESPLastLBA + 1 overflew block alignment by: " + Padding);
            }

            if (WindowsFirstLBA % BlockSize != 0)
            {
                ulong Padding = BlockSize - (WindowsFirstLBA % BlockSize);
                throw new Exception("WindowsFirstLBA overflew block alignment by: " + Padding);
            }

            if ((WindowsLastLBA + 1) % BlockSize != 0)
            {
                ulong Padding = BlockSize - ((WindowsLastLBA + 1) % BlockSize);
                throw new Exception("WindowsLastLBA + 1 overflew block alignment by: " + Padding);
            }

            Partitions.Add(new()
            {
                TypeGUID = new Guid("c12a7328-f81f-11d2-ba4b-00a0c93ec93b"),
                UID = new Guid("dec2832a-5f6c-430a-bd85-42551bce7b91"),
                FirstLBA = ESPFirstLBA,
                LastLBA = ESPLastLBA,
                Attributes = 0,
                Name = "esp"
            });

            Partitions.Add(new()
            {
                TypeGUID = new Guid("ebd0a0a2-b9e5-4433-87c0-68b6b72699c7"),
                UID = new Guid("92dee62d-ed67-4ec3-9daa-c9a4bce2c355"),
                FirstLBA = WindowsFirstLBA,
                LastLBA = WindowsLastLBA,
                Attributes = 0,
                Name = "win"
            });

            Partitions[^3].LastLBA = ESPFirstLBA - 1;

            ConsoleColor ogColor = Console.ForegroundColor;

            ulong androidSpaceInBytes = (Partitions[^3].LastLBA - Partitions[^3].FirstLBA) * SectorSize;
            ulong windowsSpaceInBytes = (Partitions[^1].LastLBA - Partitions[^1].FirstLBA) * SectorSize;

            Console.WriteLine("Resulting Allocation after Computation, Compatibility Checks and Corrections:");
            Console.WriteLine();
            Console.WriteLine($"Android: {Math.Round(androidSpaceInBytes / (double)(1024 * 1024 * 1024), 2)}GB ({Math.Round(androidSpaceInBytes / (double)(1000 * 1000 * 1000), 2)}GiB)");
            Console.WriteLine($"Windows: {Math.Round(windowsSpaceInBytes / (double)(1024 * 1024 * 1024), 2)}GB ({Math.Round(windowsSpaceInBytes / (double)(1000 * 1000 * 1000), 2)}GiB)");
            Console.WriteLine();

            Console.WriteLine("Resulting parted commands:");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine($"mkpart {Partitions[^3].Name} ext4 {Partitions[^3].FirstLBA}s {Partitions[^3].LastLBA}s");
            Console.WriteLine();
            Console.WriteLine($"mkpart {Partitions[^2].Name} fat32 {Partitions[^2].FirstLBA}s {Partitions[^2].LastLBA}s");
            Console.WriteLine();
            //Console.WriteLine($"mkpart {Partitions[^1].Name} ntfs {Partitions[^1].FirstLBA}s {Math.Truncate(Partitions[^1].LastLBA * SectorSize / (double)(1000 * 1000 * 1000))}GB");
            Console.WriteLine($"mkpart {Partitions[^1].Name} ntfs {Partitions[^1].FirstLBA}s {Partitions[^1].LastLBA}s");
            Console.WriteLine();
            Console.ForegroundColor = ogColor;
        }
    }
}
