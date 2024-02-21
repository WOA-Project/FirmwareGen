﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirmwareGen.GPT
{
    internal class GPTUtils
    {
        internal static byte[] MakeGPT(ulong DiskSize, ulong SectorSize, GPTPartition[] DefaultPartitionTable, bool IsBackupGPT = false, bool SplitInHalf = true)
        {
            ulong FirstLBA = 1;
            ulong LastLBA = (DiskSize / SectorSize) - 1;

            ulong PartitionArrayLBACount = 4;
            ulong TotalGPTLBACount = 1 /* GPT Header */ + PartitionArrayLBACount /* Partition Table */;
            ulong LastUsableLBA = LastLBA - TotalGPTLBACount;

            List<GPTPartition> Partitions = new(DefaultPartitionTable);
            Partitions[^1].LastLBA = LastUsableLBA;

            InjectWindowsPartitions(Partitions, SectorSize, 4, SplitInHalf);

            return MakeGPT(FirstLBA, LastLBA, SectorSize, [.. Partitions], PartitionArrayLBACount: PartitionArrayLBACount, IsBackupGPT: IsBackupGPT);
        }

        private static void InjectWindowsPartitions(List<GPTPartition> Partitions, ulong SectorSize, ulong BlockSize, bool SplitInHalf)
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
                ulong FourGigaBytes = 4_294_967_296 / SectorSize;
                WindowsLBACount = UsableLBACount - ESPLBACount - FourGigaBytes;

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
                throw new Exception("ESPLastLBA + 1 overflew block alignment by:: " + Padding);
            }

            if (WindowsFirstLBA % BlockSize != 0)
            {
                ulong Padding = BlockSize - (WindowsFirstLBA % BlockSize);
                throw new Exception("WindowsFirstLBA overflew block alignment by:: " + Padding);
            }

            if ((WindowsLastLBA + 1) % BlockSize != 0)
            {
                ulong Padding = BlockSize - ((WindowsLastLBA + 1) % BlockSize);
                throw new Exception("WindowsLastLBA + 1 overflew block alignment by:: " + Padding);
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
                TypeGUID = new Guid("E75CAF8F-F680-4CEE-AFA3-B001E56EFC2D"),
                UID = new Guid("92dee62d-ed67-4ec3-9daa-c9a4bce2c355"),
                FirstLBA = WindowsFirstLBA,
                LastLBA = WindowsLastLBA,
                Attributes = 0,
                Name = "win"
            });

            Partitions[^3].LastLBA = ESPFirstLBA - 1;
        }

        private static byte[] MakeGPT(ulong FirstLBA, ulong LastLBA, ulong SectorSize, GPTPartition[] Partitions, ulong PartitionArrayLBACount = 4, bool IsBackupGPT = false)
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
                DiskGUID = new Guid("efa6243a-085f-e745-f2ce-54d39ef34351"),
                PartitionArrayLBA = IsBackupGPT ? LastLBA - TotalGPTLBACount + 1 : FirstLBA + 1,
                PartitionEntryCount = 32,
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
