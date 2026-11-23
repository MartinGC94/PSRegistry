using MartinGC94.PSRegistry.API;
using MartinGC94.PSRegistry.Commands;
using MartinGC94.PSRegistry.Native;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Security;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace MartinGC94.PSRegistry
{
    internal static class Util
    {
        private static readonly Dictionary<string, RegistryHive> hiveNameTable = new Dictionary<string, RegistryHive>(StringComparer.OrdinalIgnoreCase)
        {
            {"HKCR",                  RegistryHive.ClassesRoot },
            {"HKCU",                  RegistryHive.CurrentUser },
            {"HKLM",                  RegistryHive.LocalMachine },
            {"HKU",                   RegistryHive.Users },
            {"HKCC",                  RegistryHive.CurrentConfig },
            {"HKEY_CLASSES_ROOT",     RegistryHive.ClassesRoot },
            {"HKEY_CURRENT_USER",     RegistryHive.CurrentUser },
            {"HKEY_LOCAL_MACHINE",    RegistryHive.LocalMachine },
            {"HKEY_USERS",            RegistryHive.Users },
            {"HKEY_CURRENT_CONFIG",   RegistryHive.CurrentConfig }
        };

        internal static RegistryHive GetRegistryHiveFromPath(string path, out string subKey)
        {
            Match regKeyMatch = Regex.Match(path, @"^(\w+):?\\(.*)$");

            if (!regKeyMatch.Success || !hiveNameTable.TryGetValue(regKeyMatch.Groups[1].Value, out RegistryHive baseKey))
            {
                throw new ArgumentException($"Invalid path '{path}'. Registry paths should start with the registry hive, followed by a subkey path. For example: HKLM:\\Path");
            }

            subKey = regKeyMatch.Groups[2].Value;
            return baseKey;
        }

        internal static RegistryKey OpenBaseKey(Cmdlet cmd, RegistryHive hive, string computerName, RegistryView view)
        {
            RegistryKey baseKey;
            try
            {
                baseKey = RegistryKey.OpenRemoteBaseKey(hive, computerName, view);
            }
            catch (IOException ioException)
            {
                cmd.WriteError(new IOException($"Failed to connect to machine {computerName} to open registry hive {hive}.", ioException).NewErrorRecord("BasekeyRemoteError", computerName));
                return null;
            }
            catch (Exception e)
            {
                cmd.WriteError(new ApiException($"Failed to open basekey {hive} due to error: {e.Message}", e).NewErrorRecord("BasekeyOpenError", hive));
                return null;
            }

            return baseKey;
        }

        internal static RegistryKey GetRegKeyFromPath(string path, RegistryKeyPermissionCheck permissionCheck, RegistryRights regRights)
        {
            RegistryHive hive = GetRegistryHiveFromPath(path, out string subKeyPath);
            RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
            if (string.IsNullOrEmpty(subKeyPath))
            {
                return baseKey;
            }

            RegistryKey subKey = null;
            using (baseKey)
            {
                try
                {
                    subKey = baseKey.OpenSubKey(subKeyPath, permissionCheck, regRights);
                }
                catch (SecurityException secException)
                {
                    throw new SecurityException($"User does not have the required permissions to access the key at {path}", secException);
                }
            }

            if (subKey is null)
            {
                throw new ArgumentException($"Found no registry key at {path}");
            }

            return subKey;
        }

        internal static int ToggleRestorePrivilege(Cmdlet cmd, bool enablePriv, out bool wasPreviouslyEnabled)
        {
            string action = enablePriv ? "enable" : "disable";
            int returnCode = NativeMethods.RtlAdjustPrivilege(SeRestorePrivilege, Enable: enablePriv, CurrentThread: false, out wasPreviouslyEnabled);
            if (returnCode != ERROR_SUCCESS)
            {
                cmd.WriteError(new ApiException($"Failed to {action} {nameof(SeRestorePrivilege)}.", new Win32Exception(returnCode)).NewErrorRecord("PrivilegeToggleError"));
            }

            return returnCode;
        }

        public static ErrorCategory GetErrorCategory(this Exception exception)
        {
            switch (exception)
            {
                case Win32Exception nativeError:
                    switch (nativeError.NativeErrorCode)
                    {
                        case 2:
                            return ErrorCategory.ObjectNotFound;

                        case 5:
                        case 1314:
                            return ErrorCategory.PermissionDenied;

                        case 6:
                        case 50:
                            return ErrorCategory.InvalidOperation;

                        case 87:
                        case 1610:
                            return ErrorCategory.InvalidArgument;

                        case 31:
                            return ErrorCategory.DeviceError;

                        default:
                            return ErrorCategory.NotSpecified;
                    }

                case ApiException apiError:
                    return apiError.InnerException.GetErrorCategory();

                case ArgumentNullException _:
                case ArgumentException _:
                    return ErrorCategory.InvalidArgument;

                case ItemNotFoundException _:
                    return ErrorCategory.ObjectNotFound;

                case IOException _:
                case ObjectDisposedException _:
                    return ErrorCategory.ResourceUnavailable;

                case InvalidOperationException _:
                    return ErrorCategory.InvalidOperation;

                case UnauthorizedAccessException _:
                case SecurityException _:
                    return ErrorCategory.PermissionDenied;

                case InvalidCastException _:
                    return ErrorCategory.InvalidType;

                default:
                    return ErrorCategory.NotSpecified;
            }
        }

        public static ErrorRecord NewErrorRecord(this Exception exception, string errorId, object targetObject = null)
        {
            return new ErrorRecord(exception, errorId, exception.GetErrorCategory(), targetObject);
        }

        public static void SetRecommendedAction(this ErrorRecord record, string action)
        {
            if (record.ErrorDetails is null)
            {
                record.ErrorDetails = new ErrorDetails(string.Empty);
            }

            record.ErrorDetails.RecommendedAction = action;
        }

        private const int ERROR_SUCCESS = 0;
        private const ulong SeRestorePrivilege = 18;
    }
}