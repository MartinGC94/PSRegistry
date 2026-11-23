using MartinGC94.PSRegistry.API;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security;
using System.Security.AccessControl;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.Get, "RegKey")]
    [OutputType(typeof(RegistryKey))]
    public sealed class GetRegKeyCommand : PSCmdlet
    {
        /// <summary>The path to the registry key. This only supports the full path (HKLM:\System or HKEY_LOCAL_MACHINE\System)</summary>
        [Parameter(Position = 0, Mandatory = true)]
        public string[] Path { get; set; }

        /// <summary>The computer to connect to. Default value is empty meaning that you are connecting to the local computer.</summary>
        [Parameter(Position = 1)]
        [Alias("PSComputerName")]
        [ValidateNotNull()]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        /// <summary>Get subkeys from the specified path</summary>
        [Parameter()]
        public SwitchParameter Recurse { get; set; }

        /// <summary>Defines how many sub levels to get keys from if the Recurse parameter is used.</summary>
        [Parameter()]
        [ValidateRange(0, int.MaxValue)]
        public int Depth { get; set; } = int.MaxValue;

        /// <summary>Specifies whether security checks are performed when opening registry keys and accessing their name/value pairs.</summary>
        [Parameter()]
        public RegistryKeyPermissionCheck KeyPermissionCheck { get; set; } = RegistryKeyPermissionCheck.ReadWriteSubTree;

        /// <summary>Specifies the access control rights used when opening each key.</summary>
        [Parameter()]
        public RegistryRights Rights { get; set; } = RegistryRights.FullControl;

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        private bool userSpecifiedPermissions = false;

        protected override void EndProcessing()
        {
            userSpecifiedPermissions = MyInvocation.BoundParameters.ContainsKey(nameof(KeyPermissionCheck))
                || MyInvocation.BoundParameters.ContainsKey(nameof(Rights));

            foreach (string pcName in ComputerName)
            {
                foreach (string pathItem in Path)
                {
                    RegistryHive hiveToOpen;
                    string subkeyPath;
                    try
                    {
                        hiveToOpen = Util.GetRegistryHiveFromPath(pathItem, out subkeyPath);
                    }
                    catch (ArgumentException e)
                    {
                        WriteError(e.NewErrorRecord("InvalidPath", pathItem));
                        continue;
                    }

                    RegistryKey baseKey = Util.OpenBaseKey(this, hiveToOpen, pcName, View);
                    if (baseKey is null)
                    {
                        continue;
                    }

                    RegistryKey subkey;
                    if (string.IsNullOrEmpty(subkeyPath))
                    {
                        subkey = baseKey;
                    }
                    else
                    {
                        using (baseKey)
                        {
                            subkey = OpenSubkey(baseKey, subkeyPath);
                        }

                        if (subkey is null)
                        {
                            continue;
                        }
                    }

                    if (Recurse)
                    {
                        EnumerateChildKeys(subkey, currentDepth: 0, pcName);
                    }
                    else
                    {
                        PSObject wrappedKey = subkey.WrapRegistryKey(pcName);
                        WriteObject(wrappedKey);
                    }
                }
            }
        }

        private RegistryKey OpenSubkey(RegistryKey baseKey, string subkeyPath)
        {
            string fullTargetPath = $"{baseKey.Name}\\{subkeyPath}";
            RegistryKey result;
            bool retryWithReadPermissions = false;
            try
            {
                result = baseKey.OpenSubKey(subkeyPath, KeyPermissionCheck, Rights);
            }
            catch (SecurityException secException)
            {
                if (userSpecifiedPermissions)
                {
                    WriteError(new SecurityException($"Failed to open key {fullTargetPath} due to error: {secException.Message}", secException)
                        .NewErrorRecord("ReadSubkeyWithCustomPermissions", fullTargetPath));
                    return null;
                }

                retryWithReadPermissions = true;
                result = null;
            }
            catch (Exception e)
            {
                WriteError(new ApiException($"Failed to open key {fullTargetPath} due to error: {e.Message}", e)
                    .NewErrorRecord("ReadSubkeyGenericError", fullTargetPath));
                return null;
            }

            if (retryWithReadPermissions)
            {
                try
                {
                    result = baseKey.OpenSubKey(subkeyPath, writable: false);
                }
                catch (Exception e)
                {
                    WriteError(new ApiException($"Failed to open key {fullTargetPath} due to error: {e.Message}", e)
                        .NewErrorRecord("ReadSubkeyWithReadPermissionError", fullTargetPath));
                    return null;
                }
            }

            if (result is null)
            {
                WriteError(new ItemNotFoundException($"Key {fullTargetPath} does not exist.").NewErrorRecord("InvalidSubkeyPath", fullTargetPath));
                return null;
            }

            return result;
        }

        private void EnumerateChildKeys(RegistryKey key, int currentDepth, string pcName)
        {
            string[] subkeyNames;
            try
            {
                subkeyNames = key.GetSubKeyNames();
            }
            catch (Exception e)
            {
                var exception = new ApiException($"Failed to get subkey names in '{key.Name}' due to error: {e.Message}", e);
                WriteError(exception.NewErrorRecord("GetSubkeyNameError", key.Name));
                return;
            }

            var childKeyList = new List<RegistryKey>(subkeyNames.Length);
            foreach (string name in subkeyNames)
            {
                RegistryKey childKey = OpenSubkey(key, name);
                if (childKey is null)
                {
                    continue;
                }

                childKeyList.Add(childKey);
                PSObject wrappedKey = childKey.WrapRegistryKey(pcName);
                WriteObject(wrappedKey);
            }

            int depth = currentDepth + 1;
            if (depth > Depth)
            {
                return;
            }

            foreach (RegistryKey childKey in childKeyList)
            {
                EnumerateChildKeys(childKey, depth, pcName);
            }
        }
    }
}