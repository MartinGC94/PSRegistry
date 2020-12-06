using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Security;
using System.ComponentModel;
using System.Security.AccessControl;
using System.IO;

namespace PSRegistry
{
    internal sealed class Utility
    {
        internal const char _RegPathSeparator = '\\';
        internal const string _ConfirmPrompt = "Confirm";

        /// <summary>Enum for all the different windows privileges that can be enabled.</summary>
        internal enum WindowsPrivileges : ulong
        {
            SeCreateTokenPrivilege = 1,
            SeAssignPrimaryTokenPrivilege = 2,
            SeLockMemoryPrivilege = 3,
            SeIncreaseQuotaPrivilege = 4,
            SeUnsolicitedInputPrivilege = 5,
            SeMachineAccountPrivilege = 6,
            SeTcbPrivilege = 7,
            SeSecurityPrivilege = 8,
            SeTakeOwnershipPrivilege = 9,
            SeLoadDriverPrivilege = 10,
            SeSystemProfilePrivilege = 11,
            SeSystemtimePrivilege = 12,
            SeProfileSingleProcessPrivilege = 13,
            SeIncreaseBasePriorityPrivilege = 14,
            SeCreatePagefilePrivilege = 15,
            SeCreatePermanentPrivilege = 16,
            SeBackupPrivilege = 17,
            SeRestorePrivilege = 18,
            SeShutdownPrivilege = 19,
            SeDebugPrivilege = 20,
            SeAuditPrivilege = 21,
            SeSystemEnvironmentPrivilege = 22,
            SeChangeNotifyPrivilege = 23,
            SeRemoteShutdownPrivilege = 24,
            SeUndockPrivilege = 25,
            SeSyncAgentPrivilege = 26,
            SeEnableDelegationPrivilege = 27,
            SeManageVolumePrivilege = 28,
            SeImpersonatePrivilege = 29,
            SeCreateGlobalPrivilege = 30,
            SeTrustedCredManAccessPrivilege = 31,
            SeRelabelPrivilege = 32,
            SeIncreaseWorkingSetPrivilege = 33,
            SeTimeZonePrivilege = 34,
            SeCreateSymbolicLinkPrivilege = 35
        }

        /// <summary>Dictionary for translating basekey names into their equivalent RegistryHive enum value.</summary>
        private static readonly Dictionary<string, RegistryHive> hiveNameTable = new Dictionary<string, RegistryHive>(StringComparer.OrdinalIgnoreCase)
        {
            {"HKCR",                  RegistryHive.ClassesRoot },
            {"HKCU",                  RegistryHive.CurrentUser },
            {"HKLM",                  RegistryHive.LocalMachine },
            {"HKU",                   RegistryHive.Users },
            {"HKPD",                  RegistryHive.PerformanceData },
            {"HKCC",                  RegistryHive.CurrentConfig },
            {"HKEY_CLASSES_ROOT",     RegistryHive.ClassesRoot },
            {"HKEY_CURRENT_USER",     RegistryHive.CurrentUser },
            {"HKEY_LOCAL_MACHINE",    RegistryHive.LocalMachine },
            {"HKEY_USERS",            RegistryHive.Users },
            {"HKEY_PERFORMANCE_DATA", RegistryHive.PerformanceData },
            {"HKEY_CURRENT_CONFIG",   RegistryHive.CurrentConfig }
        };

        /// <summary>Method for getting the basekey + subkeys from the path input as a Dictionary.</summary>
        internal static Dictionary<RegistryHive, List<string>> GroupKeyPathsByBaseKey(string[] regKeyPath, Cmdlet cmdlet)
        {
            Dictionary<RegistryHive, List<string>> returnDictionary = new Dictionary<RegistryHive, List<string>>();
            foreach (string path in regKeyPath)
            {
                RegistryHive baseKey;
                string[] splitPath = Regex.Split(path, "\\W");

                if (!hiveNameTable.TryGetValue(splitPath[0], out baseKey))
                {
                    ArgumentException e = new ArgumentException();
                    cmdlet.WriteError(new ErrorRecord(e, "InvalidPath",GetErrorCategory(e), path));
                }
                else
                {
                    string subKey = path.Replace(":", string.Empty).Replace(splitPath[0], string.Empty).Trim(_RegPathSeparator);
                    if (returnDictionary.ContainsKey(baseKey))
                    {
                        returnDictionary[baseKey].Add(subKey);
                    }
                    else
                    {
                        returnDictionary.Add(baseKey, new List<string>() { subKey });
                    }
                }
            }
            return returnDictionary;
        }

        /// <summary>Method for getting the basekey + subkeys from the path input as a Dictionary.</summary>
        internal static RegistryProperty[] GetRegistryProperty(RegistryKey regKey, RegistryValueOptions valueOptions, bool includeValueKind)
        {
            string[] valueNames = regKey.GetValueNames();
            RegistryValueKind valueKind = RegistryValueKind.Unknown;
            RegistryProperty[] returnData = new RegistryProperty[valueNames.Length];

            for (int i = 0; i < valueNames.Length; i++)
            {
                object value = regKey.GetValue(valueNames[i], null, valueOptions);
                if (includeValueKind)
                {
                    valueKind = regKey.GetValueKind(valueNames[i]);
                }
                returnData[i] = new RegistryProperty() { Name = valueNames[i], Type = valueKind, Value = value };
            }
            return returnData;
        }

        /// <summary>Method for getting the relevant error category based on the exception input.</summary>
        internal static ErrorCategory GetErrorCategory(Exception exception)
        {
            switch (exception)
            {
                case ArgumentNullException _:
                    return ErrorCategory.InvalidArgument;
                case ArgumentException _:
                    return ErrorCategory.InvalidArgument;
                case ObjectDisposedException _:
                    return ErrorCategory.ResourceUnavailable;
                case UnauthorizedAccessException _:
                    return ErrorCategory.PermissionDenied;
                case SecurityException _:
                    return ErrorCategory.PermissionDenied;
                case IOException _:
                    return ErrorCategory.ResourceUnavailable;
                default:
                    return ErrorCategory.NotSpecified;
            }
        }

        /// <summary>Method for adding additional members to RegistryKey objects and writing them to the Powershell pipeline.</summary>
        internal static void WriteRegKeyToPipeline(Cmdlet cmdlet, RegistryKey regKey, string pcName, bool keyOnly, RegistryValueOptions valueOptions, bool includeValueKind)
        {
            PSObject objectToWrite = new PSObject(regKey);
            objectToWrite.Members.Add(new PSNoteProperty("ComputerName", pcName), true);

            RegistryProperty[] registryProperties = new RegistryProperty[0];
            if (!keyOnly)
            {
                try
                {
                    registryProperties = GetRegistryProperty(regKey, valueOptions, includeValueKind);
                }
                catch (Exception e) when (e is PipelineStoppedException == false)
                {
                    cmdlet.WriteError(new ErrorRecord(e, "UnableToGetProperties", GetErrorCategory(e), regKey));
                }
            }
            objectToWrite.Members.Add(new PSNoteProperty("Property", registryProperties), true);

            cmdlet.WriteObject(objectToWrite);
        }

        /// <summary>Method invoking the native method for copying registry keys and optionally delete the source key</summary>
        internal static void CopyRegistryKey(RegistryKey source, RegistryKey destination ,bool deleteSource=false)
        {
            int returnCode= NativeMethods.RegCopyTree(source.Handle.DangerousGetHandle(),string.Empty,destination.Handle.DangerousGetHandle());
            if (returnCode !=0)
            {
                throw new Win32Exception(returnCode);
            }
            if (deleteSource)
            {
                string sourceComputer = PSObject.AsPSObject(source).Members.Match("ComputerName")[0].Value as string;
                if (null == sourceComputer)
                {
                    throw new ArgumentNullException("sourceComputer", "Unable to determine computer to delete key from.");
                }
                string baseKeyName=source.Name.Substring(0,source.Name.IndexOf(_RegPathSeparator));
                string subKeyName=source.Name.Substring(source.Name.IndexOf(_RegPathSeparator)+1);
                source.Dispose();
                RegistryKey baseKey=RegistryKey.OpenRemoteBaseKey(hiveNameTable[baseKeyName],sourceComputer);
                baseKey.DeleteSubKeyTree(subKeyName, true);
                baseKey.Dispose();
            }
        }

        /// <summary>Method containing the core logic for the Copy-RegKey and Move-RegKey commands.</summary>
        internal static void CopyRegistryCommand(Cmdlet cmdlet, Dictionary<RegistryHive, List<string>> groupedRegKeysToProcess)
        {
            string whatIfText        = "Will overwrite \"{0}\" with data from \"{1}\"";
            string confirmText       = "Overwrite \"{0}\" with data from \"{1}\"?";
            string whatIfTextNewKey  = "Will create \"{0}\" and copy data from \"{1}\"";
            string confirmTextNewKey = "Create \"{0}\" and copy data from \"{1}\"?";

            RegistryKey regKey;
            string[] computerName;
            RegistryView view;
            bool deleteSourceKey;
            bool disposeKeys=true;


            if (cmdlet is CopyRegKeyCommand)
            {
                CopyRegKeyCommand tempCmdlet = cmdlet as CopyRegKeyCommand;
                regKey                       = tempCmdlet.Key;
                computerName                 = tempCmdlet.ComputerName;
                view                         = tempCmdlet.View;
                deleteSourceKey              = false;
                if (tempCmdlet.DontDisposeKey)
                {
                    disposeKeys = false;
                }
            }
            else
            {
                whatIfText        = "Will move \"{1}\" to \"{0}\" - replacing any existing data.";
                confirmText       = "Move \"{1}\" to \"{0}\" - replacing any existing data?";
                whatIfTextNewKey  = "Will move \"{1}\" to \"{0}\"";
                confirmTextNewKey = "Move \"{1}\" to \"{0}\"?";

                MoveRegKeyCommand tempCmdlet = cmdlet as MoveRegKeyCommand;
                regKey                       = tempCmdlet.Key;
                computerName                 = new string[1] { tempCmdlet.ComputerName };
                view                         = tempCmdlet.View;
                deleteSourceKey              = true;
                if (tempCmdlet.DontDisposeKey)
                {
                    disposeKeys = false;
                }
            }


            foreach (string pcName in computerName)
            {
                foreach (RegistryHive hive in groupedRegKeysToProcess.Keys)
                {
                    RegistryKey baseKey;
                    try
                    {
                        cmdlet.WriteVerbose($"{pcName}: Opening base key {hive}");
                        baseKey = RegistryKey.OpenRemoteBaseKey(hive, pcName, view);
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        cmdlet.WriteError(new ErrorRecord(e, "UnableToOpenBaseKey", GetErrorCategory(e), pcName));
                        continue;
                    }

                    foreach (string subKeyPath in groupedRegKeysToProcess[hive])
                    {
                        string displayDestPath = $"{hive}{_RegPathSeparator}{subKeyPath}";
                        string actualWhatIfText;
                        string actualConfirmText;
                        RegistryKey destinationKey = null;

                        try
                        {
                            RegistryRights regRights = RegistryRights.CreateSubKey | RegistryRights.SetValue | RegistryRights.QueryValues;
                            destinationKey = baseKey.OpenSubKey(subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, regRights);

                            if (null == destinationKey)
                            {
                                actualWhatIfText = string.Format(whatIfTextNewKey, displayDestPath, regKey.Name);
                                actualConfirmText = string.Format(confirmTextNewKey, displayDestPath, regKey.Name);
                                if (cmdlet.ShouldProcess(actualWhatIfText, actualConfirmText, _ConfirmPrompt))
                                {
                                    destinationKey = baseKey.CreateSubKey(subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                                    CopyRegistryKey(regKey, destinationKey, deleteSourceKey);
                                }
                            }
                            else
                            {
                                actualWhatIfText = string.Format(whatIfText, displayDestPath, regKey.Name);
                                actualConfirmText = string.Format(confirmText, displayDestPath, regKey.Name);
                                if (cmdlet.ShouldProcess(actualWhatIfText, actualConfirmText, _ConfirmPrompt))
                                {
                                    CopyRegistryKey(regKey, destinationKey, deleteSourceKey);
                                }
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            cmdlet.WriteError(new ErrorRecord(e, "UnableToCopyKey", GetErrorCategory(e), regKey));
                        }
                        finally
                        {
                            if (disposeKeys && null != destinationKey)
                            {
                                destinationKey.Dispose();
                            }
                        }
                    }
                    baseKey.Dispose();
                }
            }
            if (disposeKeys)
            {
                regKey.Dispose();
            }
        }
    }
}