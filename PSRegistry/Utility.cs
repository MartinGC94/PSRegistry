using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Security;

namespace PSRegistry
{
    public class Utility
    {
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
    }
}