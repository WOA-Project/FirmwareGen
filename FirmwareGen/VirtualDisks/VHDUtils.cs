using DiscUtils.Streams;
using DiscUtils.Vhdx;
using System.IO;

namespace FirmwareGen.VirtualDisks
{
    internal static partial class VHDUtils
    {
        public static void CreateVHDX(string vhdfile, uint SectorSize, ulong DiskSize)
        {
            DiscUtils.Setup.SetupHelper.RegisterAssembly(typeof(Disk).Assembly);

            using Stream fs = new FileStream(vhdfile, FileMode.CreateNew, FileAccess.ReadWrite);
            using DiscUtils.VirtualDisk outDisk = Disk.InitializeDynamic(fs, Ownership.None, (long)DiskSize, DiscUtils.Geometry.FromCapacity((long)DiskSize, (int)SectorSize));
        }
    }
}
