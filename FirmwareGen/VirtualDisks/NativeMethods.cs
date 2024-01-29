using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FirmwareGen.VirtualDisks
{
    internal static class NativeMethods
    {
        public const int ERROR_SUCCESS = 0;
        public const int OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT = 1;
        public const int VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN = 0;
        public const int VIRTUAL_STORAGE_TYPE_DEVICE_VHD = 2;
        public const int VIRTUAL_STORAGE_TYPE_DEVICE_VHDX = 3;

        public static readonly Guid VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT = new("EC984AEC-A0F9-47e9-901F-71415A66345B");
        public static readonly Guid VIRTUAL_STORAGE_TYPE_VENDOR_UNKNOWN = new("00000000-0000-0000-0000-000000000000");

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int AttachVirtualDisk(nint VirtualDiskHandle, nint SecurityDescriptor, ATTACH_VIRTUAL_DISK_FLAG Flags, int ProviderSpecificFlags, ref ATTACH_VIRTUAL_DISK_PARAMETERS Parameters, nint Overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(nint hObject);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int OpenVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, string Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, OPEN_VIRTUAL_DISK_FLAG Flags, ref OPEN_VIRTUAL_DISK_PARAMETERS Parameters, ref nint Handle);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int GetVirtualDiskPhysicalPath(nint VirtualDiskHandle, ref int DiskPathSizeInBytes, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder DiskPath);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int DetachVirtualDisk(nint virtualDiskHandle, DETACH_VIRTUAL_DISK_FLAG flags, int providerSpecificFlags);
    }
}
