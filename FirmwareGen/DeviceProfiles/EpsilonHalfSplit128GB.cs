using FirmwareGen.GPT;
using System;

namespace FirmwareGen.DeviceProfiles
{
    public class EpsilonHalfSplit128GB : IDeviceProfile
    {
        public string[] GetSupplementaryBCDCommands() => [];

        public string[] GetPlatformIDs() => ["Microsoft Corporation.Surface.Surface Duo.1930", "OEMB1.*.OEMB1 Product.*", "OEMEP.*.OEMEP Product.*"];

        public string GetFFUFileName(string OSVersion, string Language, string Sku) => $"OEMEP_128GB_HalfSplit_{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";

        public string GetDriverDefinitionPath() => $@"\definitions\Desktop\ARM64\Internal\epsilon.xml";

        public ulong GetDiskTotalSize() =>
            //return 239_683_502_080; // 256GB (Bigger variant);
            //return 239_651_758_080; // 256GB (Smaller variant);
            111_723_675_648; // 128GB;

        public uint GetDiskSectorSize() => 4096;

        // OEMEP DV UFS LUN 0 Partition Layout
        public GPTPartition[] GetPartitionLayout() => [
                new()
                {
                    TypeGUID = new Guid("2c86e742-745e-4fdd-bfd8-b6a7ac638772"),
                    UID = new Guid("3e20174c-e289-a56a-39e7-2740b0043d24"),
                    FirstLBA = 6,
                    LastLBA = 7,
                    Attributes = 0,
                    Name = "ssd"
                },
                new()
                {
                    TypeGUID = new Guid("6c95e238-e343-4ba8-b489-8681ed22ad0b"),
                    UID = new Guid("3c077ac9-fe70-fb01-9fda-3e03153145ea"),
                    FirstLBA = 8,
                    LastLBA = 8199,
                    Attributes = 0,
                    Name = "persist"
                },
                new()
                {
                    TypeGUID = new Guid("988a98c9-2910-4123-aaec-1cf6b1bc28f9"),
                    UID = new Guid("b7f39878-935d-ed4b-ead8-48b159a31e04"),
                    FirstLBA = 8200,
                    LastLBA = 12295,
                    Attributes = 0,
                    Name = "metadata"
                },
                new()
                {
                    TypeGUID = new Guid("91b72d4d-71e0-4cbf-9b8e-236381cff17a"),
                    UID = new Guid("04489c96-ae78-c997-efbf-1bc9909cd1ca"),
                    FirstLBA = 12296,
                    LastLBA = 12423,
                    Attributes = 0,
                    Name = "frp"
                },
                new()
                {
                    TypeGUID = new Guid("82acc91f-357c-4a68-9c8f-689e1b1a23a1"),
                    UID = new Guid("b2ebcd63-842b-f5ff-bdf6-f630f7148b1f"),
                    FirstLBA = 12424,
                    LastLBA = 12679,
                    Attributes = 0,
                    Name = "misc"
                },
                new()
                {
                    TypeGUID = new Guid("1b81e7e6-f50d-419b-a739-2aeef8da3335"),
                    UID = new Guid("e24f3f91-ed89-c235-d6b4-afada6edb8b7"),
                    FirstLBA = 12680,
                    LastLBA = 12679,
                    Attributes = 0,
                    Name = "userdata"
                }
            ];

        public SplittingStrategy GetSplittingStrategy() => SplittingStrategy.HalfSplit;

        public Guid GetDiskGuid() => new Guid("efa6243a-085f-e745-f2ce-54d39ef34351");

        public ulong GetCustomSplittingAndroidDesiredSpace() => 4_294_967_296;
    }
}
