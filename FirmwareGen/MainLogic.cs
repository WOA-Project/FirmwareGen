using FirmwareGen.CommandLine;
using System.Diagnostics;
using System.IO;

namespace FirmwareGen
{
    public static class MainLogic
    {
        public static bool VerifyAllComponentsArePresent()
        {
            string toolDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);

            string Img2Ffu = Path.Combine(toolDirectory, "Img2Ffu.exe");

            if (!File.Exists(Img2Ffu))
            {
                Logging.Log($"Some components could not be found: {Img2Ffu}", Logging.LoggingLevel.Error);
                return false;
            }

            return true;
        }

        public static void GenerateWindowsFFU(GenerateWindowsFFUOptions options)
        {
            string toolDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName);

            string Img2Ffu = Path.Combine(toolDirectory, "Img2Ffu.exe");

            DeviceProfile deviceProfile = XmlUtils.Deserialize<DeviceProfile>(options.DeviceProfile);

            string TmpVHD = CommonLogic.GetBlankVHD(deviceProfile);

            Logging.Log("Making FFU");
            VolumeUtils.RunProgram(Img2Ffu, $@"-i {TmpVHD} -f ""{options.Output}\{deviceProfile.FFUFileName}"" -c {deviceProfile.DiskSectorSize * 4} -s {deviceProfile.DiskSectorSize} -p ""{string.Join(";", deviceProfile.PlatformIDs)}"" -o {options.WindowsVer} -b 4000 -v V2 -d VenHw(860845C1-BE09-4355-8BC1-30D64FF8E63A) -l false -e .\provisioning-partitions.txt -t ""{options.SecureBootSigningCommand}""");

            Logging.Log("Deleting Temp VHD");
            File.Delete(TmpVHD);
        }
    }
}
