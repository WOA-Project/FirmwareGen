using System;

namespace FirmwareGen.DeviceProfiles
{
    internal class ZetaHalfSplit512GB : IDeviceProfile
    {
        public byte[] GetPrimaryGPT()
        {
            throw new Exception("Not yet implemented!");
        }

        public byte[] GetBackupGPT()
        {
            throw new Exception("Not yet implemented!");
        }

        public string GetBlankVHD()
        {
            throw new Exception("Not yet implemented!");
        }

        public string[] SupplementaryBCDCommands()
        {
            return [];
        }

        public string PlatformID()
        {
            return "Microsoft Corporation.Surface.Surface Duo 2.1995;" +
                "Microsoft Corporation.Surface.Surface Duo 2.1968;" +
                "OEMC1.*.OEMC1 Product.*;" +
                "OEMZE.*.OEMZE Product.*";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"OEMZE_512GB_HalfSplit_{OSVersion}_CLIENT{Sku}_a64fre_{Language}_unsigned.ffu";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\Desktop\ARM64\Internal\zeta.xml";
        }
    }
}
