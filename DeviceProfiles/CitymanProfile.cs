﻿using System.IO;
using System.Linq;

namespace FirmwareGen.DeviceProfiles
{
    class CitymanProfile : IDeviceProfile
    {
        public string Bootloader()
        {
            return @"bin\950XL_Broad_Availability.bin";
        }

        public string DriverCommand(string DriverFolder)
        {
            return $@"{DriverFolder}\definitions\950xl.txt";
        }

        public string FFUFileName(string OSVersion, string Language, string Sku)
        {
            return $"{OSVersion}_CLIENT{Sku}_CITYMAN_A64FRE_{Language}.ffu";
        }

        public string PlatformID()
        {
            return "Microsoft.MSM8994.P6211";
        }

        public string[] SupplementaryBCDCommands()
        {
            return new string[0];
        }
    }
}
