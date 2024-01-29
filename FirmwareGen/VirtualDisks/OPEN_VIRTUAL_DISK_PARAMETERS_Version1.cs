using System.Runtime.InteropServices;

namespace FirmwareGen.VirtualDisks
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OPEN_VIRTUAL_DISK_PARAMETERS_Version1
    {
        public int RWDepth;
    }
}
