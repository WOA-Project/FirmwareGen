using FirmwareGen.GPT;
using System;

namespace FirmwareGen.DeviceProfiles
{
    internal static class Constants
    {
        // OEMEP DV UFS LUN 0 Partition Layout
        internal static readonly GPTPartition[] OEMEP_UFS_LUN_0_PARTITIONS =
        [
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

        // OEMZE MP UFS LUN 0 Partition Layout
        internal static readonly GPTPartition[] OEMZE_UFS_LUN_0_PARTITIONS =
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
        // MTP855 UFS LUN 0 Partition Layout
        internal static readonly GPTPartition[] MTP8150_UFS_LUN_0_PARTITIONS =
        [
            new()
            {
                TypeGUID = new Guid("2c86e742-745e-4fdd-bfd8-b6a7ac638772"),
                UID = new Guid("e4664002-ccca-c1e6-8d43-1283bf19dada"),
                FirstLBA = 6,
                LastLBA = 7,
                Attributes = 0,
                Name = "ssd"
            },
            new()
            {
                TypeGUID = new Guid("6c95e238-e343-4ba8-b489-8681ed22ad0b"),
                UID = new Guid("9ae53ef9-8fe5-1f9b-dcef-a044c07a0111"),
                FirstLBA = 8,
                LastLBA = 0x2007,
                Attributes = 0,
                Name = "persist"
            },
            new()
            {
                TypeGUID = new Guid("82acc91f-357c-4a68-9c8f-689e1b1a23a1"),
                UID = new Guid("1755304d-84e7-e081-d7c2-06e3dd58ba1e"),
                FirstLBA = 0x2008,
                LastLBA = 0x2107,
                Attributes = 0,
                Name = "misc"
            },
            new()
            {
                TypeGUID = new Guid("de7d4029-0f5b-41c8-ae7e-f6c023a02b33"),
                UID = new Guid("bcb1081a-c833-8f9f-f36e-36ec0f204497"),
                FirstLBA = 0x2108,
                LastLBA = 0x2187,
                Attributes = 0,
                Name = "keystore"
            },
            new()
            {
                TypeGUID = new Guid("91b72d4d-71e0-4cbf-9b8e-236381cff17a"),
                UID = new Guid("12d284d7-a08b-166e-e6fa-8958d40a7e5d"),
                FirstLBA = 0x2188,
                LastLBA = 0x2207,
                Attributes = 0,
                Name = "frp"
            },
            new()
            {
                TypeGUID = new Guid("97d7b011-54da-4835-b3c4-917ad6e73d74"),
                UID = new Guid("7a014fe0-8aeb-40b7-6c53-9ece882dd34f"),
                FirstLBA = 0x2208,
                LastLBA = 0xc2207,
                Attributes = 0x44000000000000,
                Name = "system_a"
            },
            new()
            {
                TypeGUID = new Guid("77036cd4-03d5-42bb-8ed1-37e5a88baa34"),
                UID = new Guid("220f67a2-6c3a-2e4c-5762-a2b1ce46c88f"),
                FirstLBA = 0xc2208,
                LastLBA = 0x182207,
                Attributes = 0,
                Name = "system_b"
            },
            new()
            {
                TypeGUID = new Guid("c5f681cc-9742-4641-be4e-976eeb638eee"),
                UID = new Guid("6e280891-3254-9666-e767-abdeff5148c7"),
                FirstLBA = 0x182208,
                LastLBA = 0x18a207,
                Attributes = 0x44000000000000,
                Name = "vm-system_a"
            },
            new()
            {
                TypeGUID = new Guid("77036cd4-03d5-42bb-8ed1-37e5a88baa34"),
                UID = new Guid("445d2099-5870-5ef3-2a0d-b0e1ed122fac"),
                FirstLBA = 0x18a208,
                LastLBA = 0x192207,
                Attributes = 0,
                Name = "vm-system_b"
            },
            new()
            {
                TypeGUID = new Guid("988a98c9-2910-4123-aaec-1cf6b1bc28f9"),
                UID = new Guid("99d89ebb-23f1-0b5d-468d-4aa0b9dacb24"),
                FirstLBA = 0x192208,
                LastLBA = 0x193207,
                Attributes = 0,
                Name = "metadata"
            },
            new()
            {
                TypeGUID = new Guid("66c9b323-f7fc-48b6-bf96-6f32e335a428"),
                UID = new Guid("3050c26a-6e4e-adae-7da4-e5680f9c1a89"),
                FirstLBA = 0x193208,
                LastLBA = 0x19b207,
                Attributes = 0,
                Name = "rawdump"
            },
            new()
            {
                TypeGUID = new Guid("1b81e7e6-f50d-419b-a739-2aeef8da3335"),
                UID = new Guid("6b977b5e-5ac9-6b48-38b3-f2fc9764e9b8"),
                FirstLBA = 0x19b208,
                LastLBA = 0x19b207,
                Attributes = 0,
                Name = "userdata"
            }
        ];
    }
}