using System;
using System.Runtime.InteropServices;

namespace PSRegistry
{
    internal sealed class NativeMethods
    {
        [DllImport("advapi32.dll", EntryPoint = "RegCopyTree")]
        internal static extern int RegCopyTree(
            IntPtr hKeySrc,
            [MarshalAs(UnmanagedType.LPStr), Optional] string lpSubKey,
            IntPtr hKeyDest
        );

        [DllImport("advapi32.dll", EntryPoint = "RegLoadKey")]
        internal static extern int RegLoadKey(
            IntPtr hKey,
            [MarshalAs(UnmanagedType.LPStr)] string lpSubKey,
            [MarshalAs(UnmanagedType.LPStr)] string lpFile
        );

        [DllImport("advapi32.dll", EntryPoint = "RegUnLoadKey")]
        internal static extern int RegUnLoadKey(
            IntPtr hKey,
            [MarshalAs(UnmanagedType.LPStr)] string lpSubKey
        );

        [DllImport("ntdll.dll", EntryPoint = "RtlAdjustPrivilege")]
        internal static extern int RtlAdjustPrivilege(
            ulong Privilege,
            bool Enable,
            bool CurrentThread,
            out bool Enabled
        );
    }
}