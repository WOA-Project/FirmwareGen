using System;
using System.Text;
using System.Text.RegularExpressions;

namespace FirmwareGen.VirtualDisks
{
    internal static partial class VHDUtils
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
            OPEN_VIRTUAL_DISK_PARAMETERS openParameters = new()
            {
                Version = OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1
            };
            openParameters.Version1.RWDepth = NativeMethods.OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT;

            VIRTUAL_STORAGE_TYPE openStorageType = new()
            {
                DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN,
                VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_UNKNOWN
            };

            ATTACH_VIRTUAL_DISK_PARAMETERS attachParameters = new()
            {
                Version = ATTACH_VIRTUAL_DISK_VERSION.ATTACH_VIRTUAL_DISK_VERSION_1
            };

            IntPtr handle = IntPtr.Zero;

            int openResult = NativeMethods.OpenVirtualDisk(ref openStorageType, vhdfile, VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL, OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE, ref openParameters, ref handle);

            if (openResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException($"Native error {openResult}.");
            }

            ATTACH_VIRTUAL_DISK_FLAG flags = ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME;

            if (readOnly)
            {
                flags |= ATTACH_VIRTUAL_DISK_FLAG.ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY;
            }

            int attachResult = NativeMethods.AttachVirtualDisk(handle, IntPtr.Zero, flags, 0, ref attachParameters, IntPtr.Zero);

            if (attachResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException($"Native error {attachResult}.");
            }

            int bufferSize = 260;
            StringBuilder vhdPhysicalPath = new(bufferSize);

            if (NativeMethods.GetVirtualDiskPhysicalPath(handle, ref bufferSize, vhdPhysicalPath) != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException($"Native error {attachResult}.");
            }

            _ = NativeMethods.CloseHandle(handle);

            return VHDRegex().Match(vhdPhysicalPath.ToString()).Value;
        }

        public static void UnmountVHD(string vhdfile)
        {
            OPEN_VIRTUAL_DISK_PARAMETERS openParameters = new()
            {
                Version = OPEN_VIRTUAL_DISK_VERSION.OPEN_VIRTUAL_DISK_VERSION_1
            };
            openParameters.Version1.RWDepth = NativeMethods.OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT;

            VIRTUAL_STORAGE_TYPE openStorageType = new()
            {
                DeviceId = NativeMethods.VIRTUAL_STORAGE_TYPE_DEVICE_UNKNOWN,
                VendorId = NativeMethods.VIRTUAL_STORAGE_TYPE_VENDOR_UNKNOWN
            };

            IntPtr handle = IntPtr.Zero;

            int openResult = NativeMethods.OpenVirtualDisk(ref openStorageType, vhdfile, VIRTUAL_DISK_ACCESS_MASK.VIRTUAL_DISK_ACCESS_ALL, OPEN_VIRTUAL_DISK_FLAG.OPEN_VIRTUAL_DISK_FLAG_NONE, ref openParameters, ref handle);

            if (openResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException($"Native error {openResult}.");
            }

            int dettachResult = NativeMethods.DetachVirtualDisk(handle, DETACH_VIRTUAL_DISK_FLAG.DETACH_VIRTUAL_DISK_FLAG_NONE, 0);

            if (dettachResult != NativeMethods.ERROR_SUCCESS)
            {
                throw new InvalidOperationException($"Native error {dettachResult}.");
            }

            _ = NativeMethods.CloseHandle(handle);
        }

        [GeneratedRegex(@"\d+")]
        private static partial Regex VHDRegex();
    }
}
