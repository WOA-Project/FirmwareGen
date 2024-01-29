using System.Runtime.InteropServices;

namespace FirmwareGen.VirtualDisks
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct OPEN_VIRTUAL_DISK_PARAMETERS
    {
        public OPEN_VIRTUAL_DISK_VERSION Version;
        public OPEN_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
    }
}
