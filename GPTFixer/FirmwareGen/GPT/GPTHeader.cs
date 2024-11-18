using System;

namespace GPTFixer.FirmwareGen.GPT
{
    public class GPTHeader
    {
        public string Signature
        {
            get; set;
        }

        public uint Revision
        {
            get; set;
        }

        public uint Size
        {
            get; set;
        }

        public uint CRC32
        {
            get; set;
        }

        public uint Reserved
        {
            get; set;
        }

        public ulong CurrentLBA
        {
            get; set;
        }

        public ulong BackupLBA
        {
            get; set;
        }

        public ulong FirstUsableLBA
        {
            get; set;
        }

        public ulong LastUsableLBA
        {
            get; set;
        }

        public Guid DiskGUID
        {
            get; set;
        }

        public ulong PartitionArrayLBA
        {
            get; set;
        }

        public uint PartitionEntryCount
        {
            get; set;
        }

        public uint PartitionEntrySize
        {
            get; set;
        }

        public uint PartitionArrayCRC32
        {
            get; set;
        }
    }
}