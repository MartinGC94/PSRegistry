using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Security;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace PSRegistry
{
    internal sealed class Utility
    {
        internal const char regPathSeparator = '\\';
        internal const string confirmPrompt = "Confirm";

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

        /// <summary>Splits registry paths into basekeys and subkeys, returning them in a Dictionary with the basekeys acting as the Keys and the subkeys acting as the values.</summary>
        internal static Dictionary<RegistryHive, List<string>> GroupKeyPathsByBaseKey(string[] regKeyPath, Cmdlet cmdlet)
        {
            var result = new Dictionary<RegistryHive, List<string>>();
            foreach (string path in regKeyPath)
            {
                Match baseKeyName = Regex.Match(path, @"^\w+(?=:\\|\\)");

                if (!hiveNameTable.TryGetValue(baseKeyName.Value, out RegistryHive baseKey))
                {
                    ArgumentException e = new ArgumentException("Invalid path. Registry paths should start with the registry hive, followed by a subkey path such as: HKLM:\\Path");
                    cmdlet.WriteError(new ErrorRecord(e, "InvalidPath",GetErrorCategory(e), path));
                    continue;
                }
                string subKey = path.Substring(baseKeyName.Length).Trim(':', regPathSeparator);
                if (result.ContainsKey(baseKey))
                {
                    result[baseKey].Add(subKey);
                }
                else
                {
                    result.Add(baseKey, new List<string>() { subKey });
                }
            }
            return result;
        }

        /// <summary>Gets the properties for a registry key as a dictionary.</summary>
        internal static Dictionary<string,RegistryProperty> GetRegkeyProperties(RegistryKey regKey, RegistryValueOptions valueOptions, bool includeValueKind)
        {
            string[] valueNames = regKey.GetValueNames();
            RegistryValueKind valueKind = RegistryValueKind.Unknown;
            var returnData = new Dictionary<string,RegistryProperty>(valueNames.Length);

            for (int i = 0; i < valueNames.Length; i++)
            {
                object value = regKey.GetValue(valueNames[i], null, valueOptions);
                if (includeValueKind)
                {
                    valueKind = regKey.GetValueKind(valueNames[i]);
                }
                returnData.Add(valueNames[i], new RegistryProperty() { Name = valueNames[i], ValueKind = valueKind, Value = value });
            }
            return returnData;
        }

        /// <summary>Gets a relevant error category based on the exception input.</summary>
        internal static ErrorCategory GetErrorCategory(Exception exception)
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
                            return ErrorCategory.InvalidOperation;

                        case 87:
                            return ErrorCategory.InvalidArgument;

                        default:
                            return ErrorCategory.NotSpecified;
                    }

                case ArgumentNullException _:
                case ArgumentException _:
                    return ErrorCategory.InvalidArgument;

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

        /// <summary>
        /// Unwraps any potential PSObjects from the value and converts the value to a fitting type for the valuekind.
        /// </summary>
        internal static RegistryProperty ConvertRegistryPropertyData(RegistryProperty property)
        {
            if (property.Value is PSObject wrappedObject)
            {
                property.Value = wrappedObject.BaseObject;
            }
            switch (property.ValueKind)
            {
                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    if (property.Value is null)
                    {
                        property.Value = string.Empty;
                    }
                    else if (!(property.Value is string))
                    {
                        property.Value = property.Value.ToString();
                    }
                    break;

                case RegistryValueKind.Binary:
                case RegistryValueKind.None:
                    if (!(property.Value is byte[]))
                    {
                        switch (property.Value)
                        {
                            case null:
                                property.Value = new byte[0];
                                break;

                            case string tempValue:
                                property.Value = Encoding.Unicode.GetBytes(tempValue);
                                break;

                            case int tempValue:
                                property.Value = new byte[1] { (byte)tempValue };
                                break;

                            case IList tempValue:
                                if (tempValue.Count > 0)
                                {
                                    var tempArray = new byte[tempValue.Count];
                                    for (int i = 0; i < tempValue.Count; i++)
                                    {
                                        if (tempValue[i] is PSObject wrappedByte)
                                        {
                                            tempArray[i] = Convert.ToByte(wrappedByte.BaseObject);
                                            continue;
                                        }
                                        tempArray[i] = Convert.ToByte(tempValue[i]);
                                    }
                                    property.Value = tempArray;
                                }
                                break;
                        }
                    }
                    break;

                case RegistryValueKind.DWord:
                case RegistryValueKind.QWord:
                    if (property.Value is null)
                    {
                        property.Value = 0;
                    }
                    break;

                case RegistryValueKind.MultiString:
                    if (!(property.Value is string[]))
                    {
                        switch (property.Value)
                        {
                            case null:
                                property.Value = new string[0];
                                break;

                            case string tempValue:
                                property.Value = new string[1] { tempValue };
                                break;

                            case IList tempValue:
                                if (tempValue.Count > 0)
                                {
                                    var tempArray = new string[tempValue.Count];
                                    for (int i = 0; i < tempValue.Count; i++)
                                    {
                                        tempArray[i] = tempValue[i].ToString();
                                    }
                                    property.Value = tempArray;
                                }
                                break;

                            default:
                                property.Value = new string[1] { property.Value.ToString() };
                                break;
                        }
                    }
                    break;

                default:
                    switch (property.Value)
                    {
                        case null:
                            property.Value = string.Empty;
                            break;

                        case IList tempValue:
                            if (tempValue.Count > 0)
                            {
                                if (tempValue[0] is byte || tempValue[0] is PSObject wrappedObject2 && wrappedObject2.BaseObject is byte)
                                {
                                    var tempArray = new byte[tempValue.Count];
                                    for (int i = 0; i < tempValue.Count; i++)
                                    {
                                        if (tempValue[i] is PSObject wrappedByte)
                                        {
                                            tempArray[i] = Convert.ToByte(wrappedByte.BaseObject);
                                            continue;
                                        }
                                        tempArray[i] = Convert.ToByte(tempValue[i]);
                                    }
                                    property.Value = tempArray;
                                }
                                else
                                {
                                    var tempArray = new string[tempValue.Count];
                                    for (int i = 0; i < tempValue.Count; i++)
                                    {
                                        tempArray[i] = tempValue[i].ToString();
                                    }
                                    property.Value = tempArray;
                                }
                            }
                            break;
                    }
                    break;
            }
            return property;
        }

        /// <summary>Writes registrykey objects to the pipeline and optionally wraps them as PSObjects with additional members.</summary>
        internal static void WriteRegKeyToPipeline(Cmdlet cmdlet, RegistryKey regKey, string pcName, bool keyOnly, RegistryValueOptions valueOptions, bool includeValueKind, bool asPsObject)
        {
            if (!asPsObject)
            {
                cmdlet.WriteObject(regKey);
                return;
            }
            PSObject objectToWrite = new PSObject(regKey);
            objectToWrite.TypeNames.Insert(0, "Microsoft.Win32.RegistryKey#PSRegistry");
            objectToWrite.Members.Add(new PSNoteProperty("ComputerName", pcName), preValidated:true);

            if (!keyOnly)
            {
                try
                {
                    var registryProperties = GetRegkeyProperties(regKey, valueOptions, includeValueKind);
                    objectToWrite.Members.Add(new PSNoteProperty("Properties", registryProperties), preValidated:true);
                }
                catch (Exception e) when (e is PipelineStoppedException == false)
                {
                    cmdlet.WriteError(new ErrorRecord(e, "UnableToGetProperties", GetErrorCategory(e), regKey));
                }
            }
            cmdlet.WriteObject(objectToWrite);
        }

        /// <summary>Converts string input data to RegistryKeys by running Get-RegKey.</summary>
        internal static object TransformRegPathToKey(object inputData, RegistryKeyPermissionCheck permissionCheck , RegistryRights regRights ,bool singleKeyOnly)
        {
            // Don't do anything if input isn't a string or a string collection. Also takes psobject wrapped objects into account.
            if (!(inputData is string) &&
                !(inputData is PSObject wrappedObject && wrappedObject.BaseObject is string) &&
                !(inputData is IList dataAsCollection && (dataAsCollection[0] is string || (dataAsCollection[0] is PSObject wrappedObject2 && (wrappedObject2.BaseObject is string))))
               )
            {
                return inputData;
            }

            string[] regKeyPath;
            if (inputData is IList inputCollection)
            {
                regKeyPath = new string[inputCollection.Count];
                for (int i = 0; i < inputCollection.Count; i++)
                {
                    regKeyPath[i] = inputCollection[i].ToString();
                }
            }
            else
            {
                regKeyPath = new string[1] { inputData.ToString() };
            }

            if (singleKeyOnly && regKeyPath.Length > 1)
            {
                return inputData;
            }

            var commandToRun = new GetRegKeyCommand()
            {
                Path = regKeyPath,
                KeyOnly = true,
                KeyPermissionCheck = permissionCheck,
                Rights = regRights,
                AsPsObject = false
            };
            if (singleKeyOnly)
            {
                using (var enumerator = commandToRun.Invoke<RegistryKey>().GetEnumerator())
                {
                    _ = enumerator.MoveNext();
                    return enumerator.Current;
                }
            }
            return new List<RegistryKey>(commandToRun.Invoke<RegistryKey>()).ToArray();
        }

        /// <summary>Gets the registry rights used to open an existing handle.</summary>
        internal static RegistryRights GetRegRightsFromKey(RegistryKey key)
        {
            RegistryRights result = 0;
            uint infoLength = (uint)Marshal.SizeOf(typeof(OBJECT_BASIC_INFORMATION));
            IntPtr handleInfo = Marshal.AllocHGlobal((int)infoLength);

            var status = NativeMethods.NtQueryObject(key.Handle, ObjectInformationClass.ObjectBasicInformation, handleInfo, infoLength, out _);
            if (status == NtStatus.Success)
            {
                result = ((OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(handleInfo, typeof(OBJECT_BASIC_INFORMATION))).GrantedAccess;
            }
            Marshal.FreeHGlobal(handleInfo);
            return result;
        }
    }
}