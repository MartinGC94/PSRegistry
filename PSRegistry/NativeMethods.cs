using System;
using System.Runtime.InteropServices;

namespace PSRegistry
{
    public class NativeMethods
    {
        [DllImport("advapi32.dll", EntryPoint = "RegCopyTree")]
        public static extern int RegCopyTree(
            IntPtr hKeySrc,
            [MarshalAs(UnmanagedType.LPStr), Optional] string lpSubKey,
            IntPtr hKeyDest
        );
        [DllImport("advapi32.dll", EntryPoint = "RegLoadKey")]
        public static extern int RegLoadKey(
            IntPtr hKey,
            [MarshalAs(UnmanagedType.LPStr)] string lpSubKey,
            [MarshalAs(UnmanagedType.LPStr)] string lpFile
        );
        [DllImport("advapi32.dll", EntryPoint = "RegUnLoadKey")]
            public static extern int RegUnLoadKey(
            IntPtr hKey,
            [MarshalAs(UnmanagedType.LPStr)] string lpSubKey
        );
        [DllImport("ntdll.dll", EntryPoint = "RtlAdjustPrivilege")]
        public static extern int RtlAdjustPrivilege(
            ulong Privilege,
            bool Enable,
            bool CurrentThread,
            out bool Enabled
        );
    }
}