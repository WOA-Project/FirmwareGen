using FirmwareGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace GPT2DeviceProfile
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DeviceProfile deviceProfile = GenerateDeviceProfile(args[0], 4096);

            XmlSerializer serializer = new(typeof(DeviceProfile));

            using StringWriter writer = new();
            serializer.Serialize(writer, deviceProfile);
            string xml = writer.ToString();

            Console.WriteLine(xml);
        }

        private static DeviceProfile GenerateDeviceProfile(string GPTBin, uint SectorSize)
        {
            byte[] Buffer = File.ReadAllBytes(GPTBin);
            using Stream stream = new MemoryStream(Buffer);

            GptStudio.GPT GPT = GptStudio.GPT.ReadFromStream(stream, (int)SectorSize)!;

            List<FirmwareGen.GPT.GPTPartition> deviceProfilePartitions = [];
            foreach (GptStudio.GPTPartition partition in GPT.Partitions)
            {
                FirmwareGen.GPT.GPTPartition part = new()
                {
                    UID = partition.UID,
                    TypeGUID = partition.TypeGUID,
                    FirstLBA = partition.FirstLBA,
                    LastLBA = partition.LastLBA,
                    Name = new string(partition.Name).Trim('\0'),
                    Attributes = partition.Attributes
                };
                deviceProfilePartitions.Add(part);
            }

            DeviceProfile deviceProfile = new()
            {
                DiskGuid = GPT.Header.DiskGUID,
                DiskTotalSize = (GPT.Header.LastUsableLBA + 1) * SectorSize,
                DiskSectorSize = SectorSize,
                SupplementaryBCDCommands = [],
                SplittingStrategy = SplittingStrategy.MaximizedForWindows,
                CustomSplittingAndroidDesiredSpace = 4294967296,
                PartitionLayout = [.. deviceProfilePartitions],
                PlatformIDs = ["Qualcomm.SM_PLACEHOLDER.PLACEHOLDER.*"],
                FFUFileName = "PLACEHOLDER_MaximizedForWindows.ffu",
                DriverDefinitionPath = "\\definitions\\Desktop\\ARM64\\Internal\\PLACEHOLDER.xml"
            };

            return deviceProfile;
        }
    }
}
