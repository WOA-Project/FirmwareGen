using CommandLine;
using DiscUtils;
using GPTFixer.FirmwareGen;
using GPTFixer.Img2Ffu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPTFixer
{
    internal class Program
    {
        private static (Stream InputStream, VirtualDisk? InputDisk) OpenInput(string InputFile)
        {
            Stream InputStream;
            VirtualDisk? InputDisk = null;

            if (File.Exists(InputFile) && Path.GetExtension(InputFile).Equals(".vhd", StringComparison.InvariantCultureIgnoreCase))
            {
                DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(DiscUtils.Vhd.Disk).Assembly);
                DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(DiscUtils.Vhdx.Disk).Assembly);
                InputDisk = VirtualDisk.OpenDisk(InputFile, FileAccess.ReadWrite);
                InputStream = InputDisk.Content;
            }
            else if (File.Exists(InputFile) && Path.GetExtension(InputFile).Equals(".vhdx", StringComparison.InvariantCultureIgnoreCase))
            {
                DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(DiscUtils.Vhd.Disk).Assembly);
                DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(DiscUtils.Vhdx.Disk).Assembly);
                InputDisk = VirtualDisk.OpenDisk(InputFile, FileAccess.ReadWrite);
                InputStream = InputDisk.Content;
            }
            else if (File.Exists(InputFile))
            {
                InputStream = new FileStream(InputFile, FileMode.Open, FileAccess.ReadWrite);
            }
            else
            {
                Console.WriteLine("Unknown input specified");
                throw new Exception($"Unknown Input Specified: {InputFile}");
            }

            return (InputStream, InputDisk);
        }

        private static GPT GetGPT(Stream stream, uint sectorSize)
        {
            uint BlockSize = 16384;

            byte[] GPTBuffer = new byte[BlockSize];
            stream.Seek(0, SeekOrigin.Begin);
            _ = stream.Read(GPTBuffer);

            uint requiredGPTBufferSize = Img2Ffu.GPT.GetGPTSize(GPTBuffer, sectorSize);
            if (BlockSize < requiredGPTBufferSize)
            {
                string errorMessage = $"The Block size is too small to contain the GPT, the GPT is {requiredGPTBufferSize} bytes long, the Block size is {BlockSize} bytes long";
                Console.WriteLine(errorMessage);
                //throw new Exception(errorMessage);
                BlockSize = requiredGPTBufferSize;
                GPTBuffer = new byte[BlockSize];
                stream.Seek(0, SeekOrigin.Begin);
                _ = stream.Read(GPTBuffer);
            }

            uint sectorsInABlock = BlockSize / sectorSize;

            GPT GPT = new(GPTBuffer, sectorSize);

            IOrderedEnumerable<GPT.Partition> orderedGPTPartitions = GPT.Partitions.OrderBy(x => x.FirstSector);

            if (BlockSize > requiredGPTBufferSize && orderedGPTPartitions.Any(x => x.FirstSector < sectorsInABlock))
            {
                GPT.Partition conflictingPartition = orderedGPTPartitions.First(x => x.FirstSector < sectorsInABlock);

                string errorMessage = $"The Block size is too big to contain only the GPT, the GPT is {requiredGPTBufferSize} bytes long, the Block size is {BlockSize} bytes long. The overlapping partition is {conflictingPartition.Name} at {conflictingPartition.FirstSector * sectorSize}";
                Console.WriteLine(errorMessage);
                //throw new Exception(errorMessage);
            }

            return GPT;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("GPTFixer");
            Console.WriteLine("Copyright (c) 2019-2024, Gustave Monce - gus33000.me - @gus33000");
            Console.WriteLine("Released under the MIT license at github.com/WOA-Project/FirmwareGen");
            Console.WriteLine("");

            try
            {
                _ = Parser.Default.ParseArguments<Options>(args).WithParsed(Program.FixDisk);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something happened.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }

        private static void FixDisk(Options args)
        {
            string InputFile = args.InputFile;

            Console.WriteLine("Opening input file...");
            (Stream InputStream, VirtualDisk? InputDisk) = OpenInput(InputFile);

            uint SectorSize = args.SectorSize;

            Console.WriteLine("Getting GPT from input...");
            GPT GPT = GetGPT(InputStream, SectorSize);

            Console.WriteLine("Constructing new partition list...");
            List<FirmwareGen.GPT.GPTPartition> gptPartitions = [];

            foreach (GPT.Partition partition in GPT.Partitions)
            {
                string Name = partition.Name;

                if (Name == "BS_EFIESP")
                {
                    Name = "esp";
                }
                else if (Name == "OSPool")
                {
                    Name = "win";
                }

                FirmwareGen.GPT.GPTPartition gptPartition = new()
                {
                    Attributes = partition.Attributes,
                    FirstLBA = partition.FirstSector,
                    LastLBA = partition.LastSector,
                    Name = Name,
                    UID = partition.PartitionGuid,
                    TypeGUID = partition.PartitionTypeGuid
                };

                gptPartitions.Add(gptPartition);
            }

            ulong TotalDiskSize = (ulong)InputStream.Length;
            Console.WriteLine($"Total Disk Size {TotalDiskSize:X}");

            Console.WriteLine("Constructing Primary GPT binary...");
            byte[] PrimaryGPT = CommonLogic.GetPrimaryGPT(TotalDiskSize, SectorSize, GPT.DiskGuid, [.. gptPartitions]);

            Console.WriteLine("Constructing Backup GPT binary...");
            byte[] BackupGPT = CommonLogic.GetBackupGPT(TotalDiskSize, SectorSize, GPT.DiskGuid, [.. gptPartitions]);

            Console.WriteLine("Writing Primary GPT binary...");
            InputStream.Seek(0, SeekOrigin.Begin);
            InputStream.Write(PrimaryGPT);

            Console.WriteLine("Writing Backup GPT binary...");

            InputStream.Seek((long)(TotalDiskSize - (ulong)BackupGPT.Length), SeekOrigin.Begin);
            InputStream.Write(BackupGPT);

            Console.WriteLine("Closing Input...");
            InputStream.Flush();
            InputDisk?.Dispose();
        }
    }
}