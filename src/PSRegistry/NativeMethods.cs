using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace PSRegistry
{
    internal sealed class NativeMethods
    {
        [DllImport("advapi32.dll", EntryPoint = "RegCopyTree", CharSet = CharSet.Unicode)]
        internal static extern int RegCopyTree(
            [In] SafeRegistryHandle hKeySrc,
            [In] [Optional] string lpSubKey,
            [In] SafeRegistryHandle hKeyDest
        );

        [DllImport("advapi32.dll", EntryPoint = "RegLoadKey", CharSet = CharSet.Unicode)]
        internal static extern int RegLoadKey(
            [In] SafeRegistryHandle hKey,
            [In] string lpSubKey,
            [In] string lpFile
        );

        [DllImport("advapi32.dll", EntryPoint = "RegUnLoadKey", CharSet = CharSet.Unicode)]
        internal static extern int RegUnLoadKey(
            [In] SafeRegistryHandle hKey,
            [In] string lpSubKey
        );

        [DllImport("advapi32.dll", EntryPoint = "RegRenameKey", CharSet = CharSet.Unicode)]
        internal static extern int RegRenameKey(
            [In] SafeRegistryHandle hKey,
            [In] string lpSubKeyName,
            [In] string lpNewKeyName
            );

        [DllImport("ntdll.dll", EntryPoint = "RtlAdjustPrivilege", CharSet = CharSet.Unicode)]
        internal static extern int RtlAdjustPrivilege(
            [In] WindowsPrivileges Privilege,
            [In] bool Enable,
            [In] bool CurrentThread,
            [Out] out bool Enabled
        );

        [DllImport("ntdll.dll", EntryPoint = "NtQueryObject", CharSet = CharSet.Unicode)]
        internal static extern NtStatus NtQueryObject(
            [In] SafeRegistryHandle objectHandle,
            [In] ObjectInformationClass informationClass,
            [Out] IntPtr informationPtr,
            [In] uint informationLength,
            [Out] out uint returnLength
        );
    }
}