using System.Runtime.InteropServices;

namespace FirmwareGen.VirtualDisks
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ATTACH_VIRTUAL_DISK_PARAMETERS_Version1
    {
        public int Reserved;
    }
}
