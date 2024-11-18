using System;
using System.Collections.Generic;
using System.Text;

namespace GPTFixer.FirmwareGen.GPT
{
    internal class GPTUtils
    {
        internal static byte[] MakeGPT(ulong DiskSize, ulong SectorSize, GPTPartition[] DefaultPartitionTable, Guid DiskGuid, bool IsBackupGPT = false)
        {
            ulong FirstLBA = 1;
            ulong LastLBA = DiskSize / SectorSize - 1;

            ulong PartitionArrayLBACount = 4;

            if ((ulong)DefaultPartitionTable.Length * 128 > PartitionArrayLBACount * SectorSize)
            {
                throw new Exception("Unsupported Configuration, too many partitions to fit. File an issue");
            }

            return MakeGPT(FirstLBA, LastLBA, SectorSize, DefaultPartitionTable, DiskGuid, PartitionArrayLBACount: PartitionArrayLBACount, IsBackupGPT: IsBackupGPT);
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
                PartitionTableBuffer.AddRange(new byte[Header.PartitionEntrySize * (ulong)(long)(i + 1) - (ulong)(long)PartitionTableBuffer.Count]);
            }
            PartitionTableBuffer.AddRange(new byte[Header.PartitionEntrySize * Header.PartitionEntryCount - (ulong)(long)PartitionTableBuffer.Count]);

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
            byte[] PartitionTablePaddingBuffer = new byte[(int)(PartitionArrayLBACount * SectorSize - (uint)PartitionTableBuffer.Count)];

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
