using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Security;
using System.ComponentModel;
using System.Security.AccessControl;

namespace PSRegistry
{
    public class Utility
    {
        public enum WindowsPrivileges : ulong
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
        public static Dictionary<string, RegistryHive> hiveNameTable = new Dictionary<string, RegistryHive>(StringComparer.OrdinalIgnoreCase)
        {
            {"HKCU",               RegistryHive.CurrentUser },
            {"HKLM",               RegistryHive.LocalMachine },
            {"HKU",                RegistryHive.Users },
            {"HKCR",               RegistryHive.ClassesRoot },
            {"HKEY_CURRENT_USER",  RegistryHive.CurrentUser },
            {"HKEY_LOCAL_MACHINE", RegistryHive.LocalMachine },
            {"HKEY_USERS",         RegistryHive.Users },
            {"HKEY_CLASSES_ROOT",  RegistryHive.ClassesRoot },
        };
        public static Dictionary<RegistryHive, List<string>> GroupKeyPathsByBaseKey(string[] regKeyPath, Cmdlet cmdlet)
        {
            var returnDictionary = new Dictionary<RegistryHive, List<string>>();
            foreach (string path in regKeyPath)
            {
                RegistryHive baseKey;
                string[] splitPath = Regex.Split(path, "\\W");

                if (!hiveNameTable.TryGetValue(splitPath[0], out baseKey))
                {
                    cmdlet.WriteError(
                        new ErrorRecord(
                                new ArgumentException(),
                                "InvalidPath",
                                ErrorCategory.InvalidArgument,
                                path
                            )
                        );
                }
                else
                {
                    string subKey = path.Replace(":", "").Replace(splitPath[0], "").Trim('\\');
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
        public static RegistryProperty[] GetRegistryProperty(RegistryKey regKey, RegistryValueOptions valueOptions, bool includeValueKind)
        {
            var valueNames = regKey.GetValueNames();
            var valueKind = RegistryValueKind.Unknown;
            var returnData = new RegistryProperty[valueNames.Length];

            for (int i = 0; i < valueNames.Length; i++)
            {
                var value = regKey.GetValue(valueNames[i], null, valueOptions);
                if (includeValueKind)
                {
                    valueKind = regKey.GetValueKind(valueNames[i]);
                }
                returnData[i] = new RegistryProperty() { Name = valueNames[i], Type = valueKind, Value = value };
            }
            return returnData;
        }
        public static ErrorCategory GetErrorCategory(Exception exception)
        {
            switch (exception)
            {
                case ArgumentNullException _:
                    return ErrorCategory.InvalidArgument;
                case ObjectDisposedException _:
                    return ErrorCategory.ResourceUnavailable;
                case SecurityException _:
                    return ErrorCategory.PermissionDenied;
                default:
                    return ErrorCategory.NotSpecified;
            }
        }
        public static void WriteRegKeyToPipeline(Cmdlet cmdlet, RegistryKey regKey, string pcName, bool keyOnly, RegistryValueOptions valueOptions, bool includeValueKind)
        {
            var objectToWrite = new PSObject(regKey);
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
        public static void CopyRegistryKey(RegistryKey source, RegistryKey destination, bool deleteSource=false)
        {
            int returnCode= NativeMethods.RegCopyTree(source.Handle.DangerousGetHandle(),string.Empty,destination.Handle.DangerousGetHandle());
            if (returnCode !=0)
            {
                throw new Win32Exception(returnCode);
            }
            if (deleteSource)
            {

            }
        }
        
        public static void CopyRegistryCommand(Cmdlet cmdlet, Dictionary<RegistryHive, List<string>> groupedRegKeysToProcess)
        {
            string whatIfText        = "Will overwrite \"{0}\" with data from \"{1}\"";
            string confirmText       = "Overwrite \"{0}\" with data from \"{1}\"?";
            string whatIfTextNewKey  = "Will create \"{0}\" and copy data from \"{1}\"";
            string confirmTextNewKey = "Create \"{0}\" and copy data from \"{1}\"?";

            RegistryKey regKey;
            string[] computerName;
            RegistryView view;
            bool deleteSourceKey;


            if (cmdlet is CopyRegKeyCommand)
            {
                var tempCmdlet          = cmdlet as CopyRegKeyCommand;
                regKey                  = tempCmdlet.RegKey;
                computerName            = tempCmdlet.ComputerName;
                view                    = tempCmdlet.View;
                deleteSourceKey         = false;
            }
            else
            {
                whatIfText        = "Will move \"{1}\" to \"{0}\" - replacing any existing data.";
                confirmText       = "Move \"{1}\" to \"{0}\" - replacing any existing data?";
                whatIfTextNewKey  = "Will move \"{1}\" to \"{0}\"";
                confirmTextNewKey = "Move \"{1}\" to \"{1}\"?";

                var tempCmdlet          = cmdlet as MoveRegKeyCommand;
                regKey                  = tempCmdlet.RegKey;
                computerName            = tempCmdlet.ComputerName;
                view                    = tempCmdlet.View;
                deleteSourceKey         = true;
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
                        //TODO: Get friendly hive name.
                        string displayDestPath = $"{hive}\\{subKeyPath}";
                        string actualWhatIfText;
                        string actualConfirmText;

                        try
                        {
                            var regRights = RegistryRights.CreateSubKey | RegistryRights.SetValue | RegistryRights.QueryValues;
                            var destinationKey = baseKey.OpenSubKey(subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, regRights);

                            if (null == destinationKey)
                            {
                                actualWhatIfText = string.Format(whatIfTextNewKey, displayDestPath, regKey.Name);
                                actualConfirmText = string.Format(confirmTextNewKey, displayDestPath, regKey.Name);
                                if (cmdlet.ShouldProcess(actualWhatIfText, actualConfirmText, "Confirm"))
                                {
                                    destinationKey = baseKey.CreateSubKey(subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                                    CopyRegistryKey(regKey, destinationKey,deleteSourceKey);
                                }
                            }
                            else
                            {
                                actualWhatIfText = string.Format(whatIfText, displayDestPath, regKey.Name);
                                actualConfirmText = string.Format(confirmText, displayDestPath, regKey.Name);
                                if (cmdlet.ShouldProcess(actualWhatIfText, actualConfirmText, "Confirm"))
                                {
                                    CopyRegistryKey(regKey, destinationKey,deleteSourceKey);
                                }
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            cmdlet.WriteError(new ErrorRecord(e, "UnableToCopyKey", GetErrorCategory(e), regKey));
                        }
                    }
                    baseKey.Dispose();
                }
            }
        }
    }
}