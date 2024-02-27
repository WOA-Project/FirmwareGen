using FirmwareGen.GPT;
using System;

namespace FirmwareGen.DeviceProfiles
{
    public class ZetaHalfSplit128GB : IDeviceProfile
    {
        public string[] GetSupplementaryBCDCommands()
        {
            return [];
        }

        public string[] GetPlatformIDs()
        {
            return ["Microsoft Corporation.Surface.Surface Duo 2.1995",
                "Microsoft Corporation.Surface.Surface Duo 2.1968",
                "OEMC1.*.OEMC1 Product.*",
                "OEMZE.*.OEMZE Product.*"];
        }

        public string GetFFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"OEMZE_128GB_HalfSplit_{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string GetDriverDefinitionPath(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\zeta.xml";
        }

        public ulong GetDiskTotalSize()
        {
            return 110_394_081_280; // 128GB;
        }

        public uint GetDiskSectorSize()
        {
            return 4096;
        }

        // OEMZE MP UFS LUN 0 Partition Layout
        public GPTPartition[] GetPartitionLayout()
        {
            return
            [
                    new()
                {
                    TypeGUID = new Guid("2c86e742-745e-4fdd-bfd8-b6a7ac638772"),
                    UID = new Guid("dd334fea-6336-1cfc-9b54-625a4f119d3b"),
                    FirstLBA = 6,
                    LastLBA = 7,
                    Attributes = 0,
                    Name = "ssd"
                },
                new()
                {
                    TypeGUID = new Guid("6c95e238-e343-4ba8-b489-8681ed22ad0b"),
                    UID = new Guid("9c508206-a952-e02d-73dd-25b08eda76e4"),
                    FirstLBA = 8,
                    LastLBA = 8199,
                    Attributes = 0,
                    Name = "persist"
                },
                new()
                {
                    TypeGUID = new Guid("988a98c9-2910-4123-aaec-1cf6b1bc28f9"),
                    UID = new Guid("5e514cb8-1151-c29c-830f-5bf399b40c74"),
                    FirstLBA = 8200,
                    LastLBA = 12295,
                    Attributes = 0,
                    Name = "metadata"
                },
                new()
                {
                    TypeGUID = new Guid("91b72d4d-71e0-4cbf-9b8e-236381cff17a"),
                    UID = new Guid("dd11345e-4c1f-99ee-afd0-e55e6ecc92d6"),
                    FirstLBA = 12296,
                    LastLBA = 12423,
                    Attributes = 0,
                    Name = "frp"
                },
                new()
                {
                    TypeGUID = new Guid("82acc91f-357c-4a68-9c8f-689e1b1a23a1"),
                    UID = new Guid("035e02e0-4bb9-06ce-4851-a1ec88834507"),
                    FirstLBA = 12424,
                    LastLBA = 12679,
                    Attributes = 0,
                    Name = "misc"
                },
                new()
                {
                    TypeGUID = new Guid("66c9b323-f7fc-48b6-bf96-6f32e335a428"),
                    UID = new Guid("68733ebb-ef36-f8ca-11e9-893ace2e6585"),
                    FirstLBA = 12680,
                    LastLBA = 89479,
                    Attributes = 0,
                    Name = "rawdump"
                },
                new()
                {
                    TypeGUID = new Guid("21adb864-c9e7-4c76-be68-568e20c58439"),
                    UID = new Guid("d296e468-eba2-dfe6-23d5-3b5cb3a853d9"),
                    FirstLBA = 89480,
                    LastLBA = 97835,
                    Attributes = 0,
                    Name = "vm-data"
                },
                new()
                {
                    TypeGUID = new Guid("1b81e7e6-f50d-419b-a739-2aeef8da3335"),
                    UID = new Guid("411d7753-d116-25ee-ca31-6436766e9580"),
                    FirstLBA = 97836,
                    LastLBA = 97835,
                    Attributes = 0,
                    Name = "userdata"
                }
            ];
        }

        public SplittingStrategy GetSplittingStrategy()
        {
            return SplittingStrategy.HalfSplit;
        }

        public Guid GetDiskGuid()
        {
            return new Guid("efa6243a-085f-e745-f2ce-54d39ef34351");
        }

        public ulong GetCustomSplittingAndroidDesiredSpace()
        {
            return 4_294_967_296;
        }
    }
}
