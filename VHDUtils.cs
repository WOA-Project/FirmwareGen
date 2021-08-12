using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace FirmwareGen
{
    internal static class VHDUtils
    {
        /// <summary>
        ///     Mounts FFU or VHD files on a target system.
        /// </summary>
        /// <param name="vhdfile">A path as a string to a FFU or VHD file to mount on the current system.</param>
        /// <param name="flag">If set to true, the function will mount a FFU image and not a VHD image.</param>
        /// <returns>
        ///     A string array containing for the first parameter the full path to the junction linked to the image, and for
        ///     the second parameter the Physical disk id path.
        /// </returns>
        public static string MountVHD(string vhdfile, bool readOnly)
        {
            NativeMethods.OPEN_VIRTUAL_DISK_PARAMETERS openParameters = new()
            {
                Version = NativeMethods.OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1
            };
            openParameters.Version1.RWDepth = NativeMethods.OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT;

            NativeMethods.VIRTUAL_STORAGE_TYPE openStorageType = new()
            {
                DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN,
                VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_UNKNOWN
            };

            NativeMethods.ATTACH_VIRTUAL_DISK_PARAMETERS attachParameters = new()
            {
                Version = NativeMethods.ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_1
            };

            IntPtr handle = IntPtr.Zero;

            int openResult = NativeMethods.OpenVirtualDisk(ref openStorageType, vhdfile, NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL, NativeMethods.OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE, ref openParameters, ref handle);

            if (openResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", openResult));
            }

            NativeMethods.ATTACH_VIRTUAL_DISK_FLAG flags = NativeMethods.ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME;

            if (readOnly)
            {
                flags |= NativeMethods.ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY;
            }

            int attachResult = NativeMethods.AttachVirtualDisk(handle, IntPtr.Zero, flags, 0, ref attachParameters, IntPtr.Zero);

            if (attachResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", attachResult));
            }

            int bufferSize = 260;
            StringBuilder vhdPhysicalPath = new(bufferSize);

            if (NativeMethods.GetVirtualDiskPhysicalPath(handle, ref bufferSize, vhdPhysicalPath) != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", attachResult));
            }

            NativeMethods.CloseHandle(handle);

            return Regex.Match(vhdPhysicalPath.ToString(), @"\d+").Value;
        }

        public static void UnmountVHD(string vhdfile)
        {
            NativeMethods.OPEN_VIRTUAL_DISK_PARAMETERS openParameters = new()
            {
                Version = NativeMethods.OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1
            };
            openParameters.Version1.RWDepth = NativeMethods.OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT;

            NativeMethods.VIRTUAL_STORAGE_TYPE openStorageType = new()
            {
                DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN,
                VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_UNKNOWN
            };

            IntPtr handle = IntPtr.Zero;

            int openResult = NativeMethods.OpenVirtualDisk(ref openStorageType, vhdfile, NativeMethods.VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL, NativeMethods.OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE, ref openParameters, ref handle);

            if (openResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", openResult));
            }

            int dettachResult = NativeMethods.DetachVirtualDisk(handle, NativeMethods.DETACH_VIRTUAL_DISK_FLAG.DETACH_VIRTUAL_DISK_FLAG_NONE, 0);

            if (dettachResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Native error {0}.", dettachResult));
            }

            NativeMethods.CloseHandle(handle);
        }
    }

    internal static class NativeMethods
    {
        public enum ATTACH_VIRTUAL_DISK_FLAG
        {
            ATTACH_VIRTUAL_DISK_FLAG_NONE = 0x00000000,
            ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY = 0x00000001,
            ATTACH_VIRTUAL_DISK_FLAG_NO_DRIVE_LETTER = 0x00000002,
            ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME = 0x00000004,
            ATTACH_VIRTUAL_DISK_FLAG_NO_LOCAL_HOST = 0x00000008
        }

        public enum ATTACH_VIRTUAL_DISK_VERSION
        {
            ATTACH_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,
            ATTACH_VIRTUAL_DISK_VERSION_1 = 1
        }

        public enum OPEN_VIRTUAL_DISK_FLAG
        {
            OPEN_VIRTUAL_DISK_FLAG_NONE = 0x00000000,
            OPEN_VIRTUAL_DISK_FLAG_NO_PARENTS = 0x00000001,
            OPEN_VIRTUAL_DISK_FLAG_BLANK_FILE = 0x00000002,
            OPEN_VIRTUAL_DISK_FLAG_BOOT_DRIVE = 0x00000004
        }

        public enum OPEN_VIRTUAL_DISK_VERSION
        {
            OPEN_VIRTUAL_DISK_VERSION_1 = 1
        }

        public enum VIRTUAL_DISK_ACCESS_MASK
        {
            VIRTUAL_DISK_ACCESS_ATTACH_RO = 0x00010000,
            VIRTUAL_DISK_ACCESS_ATTACH_RW = 0x00020000,
            VIRTUAL_DISK_ACCESS_DETACH = 0x00040000,
            VIRTUAL_DISK_ACCESS_GET_INFO = 0x00080000,
            VIRTUAL_DISK_ACCESS_READ = 0x000d0000,
            VIRTUAL_DISK_ACCESS_CREATE = 0x00100000,
            VIRTUAL_DISK_ACCESS_METAOPS = 0x00200000,
            VIRTUAL_DISK_ACCESS_WRITABLE = 0x00320000,
            VIRTUAL_DISK_ACCESS_ALL = 0x003f0000
        }

        public enum DETACH_VIRTUAL_DISK_FLAG
        {
            DETACH_VIRTUAL_DISK_FLAG_NONE = 0x00000000
        }

        public const int ERROR_SUCCESS = 0;
        public const int OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT = 1;
        public const int VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN = 0;
        public const int VIRTUAL_STORAGE_TYPE_DEVICE_VHD = 2;
        public const int VIRTUAL_STORAGE_TYPE_DEVICE_VHDX = 3;

        public static readonly Guid VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT = new("EC984AEC-A0F9-47e9-901F-71415A66345B");
        public static readonly Guid VIRTUAL_STORAGE_TYPE_VENDOR_UNKNOWN = new("00000000-0000-0000-0000-000000000000");

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int AttachVirtualDisk(IntPtr VirtualDiskHandle, IntPtr SecurityDescriptor, ATTACH_VIRTUAL_DISK_FLAG Flags, int ProviderSpecificFlags, ref ATTACH_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int OpenVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, string Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, OPEN_VIRTUAL_DISK_FLAG Flags, ref OPEN_VIRTUAL_DISK_PARAMETERS Parameters, ref IntPtr Handle);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int GetVirtualDiskPhysicalPath(IntPtr VirtualDiskHandle, ref int DiskPathSizeInBytes, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder DiskPath);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int DetachVirtualDisk(IntPtr virtualDiskHandle, DETACH_VIRTUAL_DISK_FLAG flags, Int32 providerSpecificFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ATTACH_VIRTUAL_DISK_PARAMETERS
        {
            public ATTACH_VIRTUAL_DISK_VERSION Version;
            public ATTACH_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ATTACH_VIRTUAL_DISK_PARAMETERS_Version1
        {
            public int Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OPEN_VIRTUAL_DISK_PARAMETERS
        {
            public OPEN_VIRTUAL_DISK_VERSION Version;
            public OPEN_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OPEN_VIRTUAL_DISK_PARAMETERS_Version1
        {
            public int RWDepth;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct VIRTUAL_STORAGE_TYPE
        {
            public int DeviceId;
            public Guid VendorId;
        }
    }
}
