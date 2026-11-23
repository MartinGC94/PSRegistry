using MartinGC94.PSRegistry.API;
using MartinGC94.PSRegistry.Native.Enums;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;


namespace MartinGC94.PSRegistry.Native
{
    internal class NativeMethods
    {
        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RegGetValueW(
            SafeRegistryHandle hkey,
            string lpSubKey,
            string lpValue,
            DwFlags dwFlags,
            out RegistryValueType pdwType,
            byte[] pvData,
            ref int pcbData);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RegQueryInfoKeyW(
            SafeRegistryHandle hKey,
            IntPtr lpClass,
            IntPtr lpcchClass,
            IntPtr lpReserved,
            IntPtr lpcSubKeys,
            IntPtr lpcbMaxSubKeyLen,
            IntPtr lpcbMaxClassLen,
            IntPtr lpcValues,
            IntPtr lpcbMaxValueNameLen,
            IntPtr lpcbMaxValueLen,
            IntPtr lpcbSecurityDescriptor,
            out System.Runtime.InteropServices.ComTypes.FILETIME lpftLastWriteTime);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RegLoadKeyW(SafeRegistryHandle hKey, string lpSubKey, string lpFile);

        [DllImport("ntdll.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RtlAdjustPrivilege(ulong Privilege, bool Enable, bool CurrentThread, out bool Enabled);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RegUnLoadKeyW(SafeRegistryHandle hKey, string lpSubKey);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RegSetKeyValueW(
            SafeRegistryHandle hkey,
            string lpSubKey,
            string lpValueName,
            RegistryValueType dwType,
            byte[] lpData,
            int cbData);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RegCopyTreeW(SafeRegistryHandle hKeySrc, string lpSubKey, SafeRegistryHandle hKeyDest);

        [DllImport("advapi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern int RegRenameKey(SafeRegistryHandle hKey, string lpSubKeyName, string lpNewKeyName);
    }
}