using MartinGC94.PSRegistry.Commands;
using MartinGC94.PSRegistry.Native;
using MartinGC94.PSRegistry.Native.Enums;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;

namespace MartinGC94.PSRegistry.API
{
    public static class RegKeyETS
    {
        internal static PSObject WrapRegistryKey(this RegistryKey key, string pcName)
        {
            PSObject wrappedKey = new PSObject(key);
            wrappedKey.TypeNames.Insert(0, "Microsoft.Win32.RegistryKey#PSRegistry");
            wrappedKey.Properties.Add(new PSNoteProperty("ComputerName", pcName), preValidated: true);
            wrappedKey.Properties.Add(new PSCodeProperty("Properties", typeof(RegKeyETS).GetMethod(nameof(GetProperties))), preValidated: true);
            wrappedKey.Properties.Add(new PSCodeProperty("LastWriteTime", typeof(RegKeyETS).GetMethod(nameof(GetLastWriteTime))), preValidated: true);
            wrappedKey.Methods.Add(new PSCodeMethod("GetProperty", typeof(RegKeyETS).GetMethod(nameof(GetProperty))), preValidated: true);

            return wrappedKey;
        }

        public static Dictionary<string, RegistryProperty> GetProperties(PSObject obj)
        {
            RegistryKey unwrappedKey = obj.BaseObject as RegistryKey ?? throw new ArgumentException("The wrapped object did not contain a RegistryKey.");
            string[] propertyNames = unwrappedKey.GetValueNames();

            var result = new Dictionary<string, RegistryProperty>(propertyNames.Length);
            foreach (string name in propertyNames)
            {
                var property = GetProperty(obj, name);
                result.Add(name, property);
            }

            return result;
        }

        public static RegistryProperty GetProperty(PSObject obj, string propertyName)
        {
            RegistryKey unwrappedKey = obj.BaseObject as RegistryKey ?? throw new ArgumentException("The wrapped object did not contain a RegistryKey.");
            int returnCode;
            RegistryValueType propertyType;
            int bufferSize = 1024;
            byte[] outputBuffer = new byte[bufferSize];

            while (true)
            {
                returnCode = NativeMethods.RegGetValueW(
                    unwrappedKey.Handle,
                    lpSubKey: null,
                    propertyName,
                    DwFlags.RRF_RT_ANY | DwFlags.RRF_NOEXPAND,
                    out propertyType,
                    outputBuffer,
                    ref bufferSize);

                if (returnCode != ERROR_MORE_DATA)
                {
                    break;
                }

                outputBuffer = new byte[bufferSize];
            }

            if (returnCode != ERROR_SUCCESS)
            {
                throw new Win32Exception(returnCode);
            }

            var outputArray = new byte[bufferSize];
            Array.Copy(outputBuffer, outputArray, bufferSize);
            return new RegistryProperty(propertyName, outputArray, propertyType);
        }

        public static DateTime GetLastWriteTime(PSObject obj)
        {
            RegistryKey unwrappedKey = obj.BaseObject as RegistryKey ?? throw new ArgumentException("The wrapped object did not contain a RegistryKey.");

            int returnCode = NativeMethods.RegQueryInfoKeyW(
                    unwrappedKey.Handle,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    out System.Runtime.InteropServices.ComTypes.FILETIME lastWriteTime);
            if (returnCode != ERROR_SUCCESS)
            {
                throw new Win32Exception(returnCode);
            }

            long value = ((long)lastWriteTime.dwHighDateTime << 32) | (uint)lastWriteTime.dwLowDateTime;
            return DateTime.FromFileTimeUtc(value);
        }

        private const int ERROR_SUCCESS = 0;
        private const int ERROR_MORE_DATA = 234;
    }
}