using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirmwareGen.GPT
{
    internal class GPTUtils
    {
        internal static byte[] MakeGPT(ulong DiskSize, ulong SectorSize, GPTPartition[] DefaultPartitionTable, Guid DiskGuid, bool IsBackupGPT = false, bool SplitInHalf = true, ulong AndroidDesiredSpace = 4_294_967_296)
        {
            ulong FirstLBA = 1;
            ulong LastLBA = (DiskSize / SectorSize) - 1;

            ulong PartitionArrayLBACount = 4;

            if ((ulong)DefaultPartitionTable.Length * 128 > PartitionArrayLBACount * SectorSize)
            {
                throw new Exception("Unsupported Configuration, too many partitions to fit. File an issue");
            }

            ulong TotalGPTLBACount = 1 /* GPT Header */ + PartitionArrayLBACount /* Partition Table */;
            ulong LastUsableLBA = LastLBA - TotalGPTLBACount;

            List<GPTPartition> Partitions = new(DefaultPartitionTable);
            Partitions[^1].LastLBA = LastUsableLBA;

            if (AndroidDesiredSpace < 4_294_967_296)
            {
                throw new Exception("ERROR");
            }

            InjectWindowsPartitions(Partitions, SectorSize, 4, SplitInHalf, AndroidDesiredSpace);

            return MakeGPT(FirstLBA, LastLBA, SectorSize, [.. Partitions], DiskGuid, PartitionArrayLBACount: PartitionArrayLBACount, IsBackupGPT: IsBackupGPT);
        }

        private static void InjectWindowsPartitions(List<GPTPartition> Partitions, ulong SectorSize, ulong BlockSize, bool SplitInHalf, ulong AndroidDesiredSpace = 4_294_967_296)
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

            ulong WindowsLBACount;

            /* Strategy to reserve half for Android, half for Windows */
            if (SplitInHalf)
            {
                ulong AndroidOtherLUNLBAUsage = 8_679_372 /* Size taken in Android by another LUN that counts towards Android space utilization */;
                WindowsLBACount = (UsableLBACount + AndroidOtherLUNLBAUsage - ESPLBACount) / 2;

                // Windows System Requirement Specifications mandate the Windows Partition must 
                // be at the very least 64GB in size for meeting the minimum Windows Specification 
                // requirements. It is a violation of the Windows minimum Specification requirements
                // to override this value for Windows 11 Products and your device will not be
                // compatible or supported for Windows 11 if this gets changed.
                if (WindowsLBACount < SixtyFourGigaBytes)
                {
                    WindowsLBACount = SixtyFourGigaBytes;
                }

                // In the case of the 4GB for Android strategy, we cannot do this or we risk to get userdata < 4GB
                if (WindowsLBACount % BlockSize != 0)
                {
                    WindowsLBACount += BlockSize - (WindowsLBACount % BlockSize);
                }
            }
            /* Strategy to reserve 4GB for Android Only */
            else
            {
                ulong FourGigaBytes = AndroidDesiredSpace / SectorSize;
                WindowsLBACount = UsableLBACount - ESPLBACount - FourGigaBytes;

                // Windows System Requirement Specifications mandate the Windows Partition must 
                // be at the very least 64GB in size for meeting the minimum Windows Specification 
                // requirements. It is a violation of the Windows minimum Specification requirements
                // to override this value for Windows 11 Products and your device will not be
                // compatible or supported for Windows 11 if this gets changed.
                if (WindowsLBACount < SixtyFourGigaBytes)
                {
                    WindowsLBACount = SixtyFourGigaBytes;
                }

                if (WindowsLBACount % BlockSize != 0)
                {
                    WindowsLBACount -= WindowsLBACount % BlockSize;
                }
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

        private static byte[] MakeGPT(ulong FirstLBA, ulong LastLBA, ulong SectorSize, GPTPartition[] Partitions, Guid DiskGuid, ulong PartitionArrayLBACount = 4, bool IsBackupGPT = false)
        {
            // -------------------
            // 0: Reserved/MBR
            // -------------------
            // 1: GPT Header
            // -------------------
            // 2: Partition Table
            // 3: Partition Table
            // 4: Partition Table
            // 5: Partition Table
            // -------------------
            // 6: First Usable LBA
            // ...
            // -5: Last Usable LBA
            // -------------------
            // -4: Partition Table
            // -3: Partition Table
            // -2: Partition Table
            // -1: Partition Table
            // -------------------
            // -0: Backup GPT Header
            // -------------------

            ulong TotalGPTLBACount = 1 /* GPT Header */ + PartitionArrayLBACount /* Partition Table */;

            ulong FirstUsableLBA = FirstLBA + TotalGPTLBACount;
            ulong LastUsableLBA = LastLBA - TotalGPTLBACount;

            uint PartitionEntryCount;

            if ((uint)Partitions.Length > 128)
            {
                throw new Exception("Unsupported Configuration, too many partitions than supported, please file an issue.");
            }
            else
            {
                PartitionEntryCount = (uint)Partitions.Length > 64 ? 128 : (uint)Partitions.Length > 32 ? 64 : (uint)32;
            }

            GPTHeader Header = new()
            {
                Signature = "EFI PART",
                Revision = 0x10000,
                Size = 92,
                CRC32 = 0,
                Reserved = 0,
                CurrentLBA = IsBackupGPT ? LastLBA : FirstLBA,
                BackupLBA = IsBackupGPT ? FirstLBA : LastLBA,
                FirstUsableLBA = FirstUsableLBA,
                LastUsableLBA = LastUsableLBA,
                DiskGUID = DiskGuid,
                PartitionArrayLBA = IsBackupGPT ? LastLBA - TotalGPTLBACount + 1 : FirstLBA + 1,
                PartitionEntryCount = PartitionEntryCount,
                PartitionEntrySize = 128,
                PartitionArrayCRC32 = 0
            };

            List<byte> PartitionTableBuffer = [];
            for (int i = 0; i < Partitions.Length; i++)
            {
                PartitionTableBuffer.AddRange(Partitions[i].TypeGUID.ToByteArray());
                PartitionTableBuffer.AddRange(Partitions[i].UID.ToByteArray());
                PartitionTableBuffer.AddRange(BitConverter.GetBytes(Partitions[i].FirstLBA));
                PartitionTableBuffer.AddRange(BitConverter.GetBytes(Partitions[i].LastLBA));
                PartitionTableBuffer.AddRange(BitConverter.GetBytes(Partitions[i].Attributes));
                PartitionTableBuffer.AddRange(Encoding.Unicode.GetBytes(Partitions[i].Name));
                PartitionTableBuffer.AddRange(new byte[(Header.PartitionEntrySize * (ulong)(long)(i + 1)) - (ulong)(long)PartitionTableBuffer.Count]);
            }
            PartitionTableBuffer.AddRange(new byte[(Header.PartitionEntrySize * Header.PartitionEntryCount) - (ulong)(long)PartitionTableBuffer.Count]);

            uint PartitionTableCRC32 = CRC32.Compute([.. PartitionTableBuffer], 0, (uint)PartitionTableBuffer.Count);
            Header.PartitionArrayCRC32 = PartitionTableCRC32;

            byte[] HeaderBuffer =
            [
                .. Encoding.ASCII.GetBytes(Header.Signature),
                .. BitConverter.GetBytes(Header.Revision),
                .. BitConverter.GetBytes(Header.Size),
                .. BitConverter.GetBytes(Header.CRC32),
                .. BitConverter.GetBytes(Header.Reserved),
                .. BitConverter.GetBytes(Header.CurrentLBA),
                .. BitConverter.GetBytes(Header.BackupLBA),
                .. BitConverter.GetBytes(Header.FirstUsableLBA),
                .. BitConverter.GetBytes(Header.LastUsableLBA),
                .. Header.DiskGUID.ToByteArray(),
                .. BitConverter.GetBytes(Header.PartitionArrayLBA),
                .. BitConverter.GetBytes(Header.PartitionEntryCount),
                .. BitConverter.GetBytes(Header.PartitionEntrySize),
                .. BitConverter.GetBytes(Header.PartitionArrayCRC32),
            ];

            Header.CRC32 = CRC32.Compute(HeaderBuffer, 0, (uint)HeaderBuffer.Length);
            byte[] bytes = BitConverter.GetBytes(Header.CRC32);

            HeaderBuffer[16] = bytes[0];
            HeaderBuffer[17] = bytes[1];
            HeaderBuffer[18] = bytes[2];
            HeaderBuffer[19] = bytes[3];

            byte[] HeaderPaddingBuffer = new byte[(int)(SectorSize - (uint)HeaderBuffer.Length)];
            byte[] PartitionTablePaddingBuffer = new byte[(int)((PartitionArrayLBACount * SectorSize) - (uint)PartitionTableBuffer.Count)];

            List<byte> GPTBuffer = [];
            if (IsBackupGPT)
            {
                GPTBuffer.AddRange(PartitionTableBuffer);
                GPTBuffer.AddRange(PartitionTablePaddingBuffer);

                GPTBuffer.AddRange(HeaderBuffer);
                GPTBuffer.AddRange(HeaderPaddingBuffer);
            }
            else
            {
                GPTBuffer.AddRange(HeaderBuffer);
                GPTBuffer.AddRange(HeaderPaddingBuffer);

                GPTBuffer.AddRange(PartitionTableBuffer);
                GPTBuffer.AddRange(PartitionTablePaddingBuffer);
            }

            return [.. GPTBuffer];
        }
    }
}
