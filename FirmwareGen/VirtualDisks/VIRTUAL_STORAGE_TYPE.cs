using System;
using System.Runtime.InteropServices;

namespace FirmwareGen.VirtualDisks
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct VIRTUAL_STORAGE_TYPE
    {
        public int DeviceId;
        public Guid VendorId;
    }
}
