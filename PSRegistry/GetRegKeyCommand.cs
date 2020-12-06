using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.AccessControl;

namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Get, "RegKey")]
    [OutputType(typeof(RegistryKey))]

    public sealed class GetRegKeyCommand : Cmdlet
    {
        private Dictionary<RegistryHive, List<string>> _GroupedRegKeysToProcess;

        #region Parameters
        /// <summary>The path to the registry key. This only supports the full path (HKLM:\System or HKEY_LOCAL_MACHINE\System)</summary>
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] Path { get; set; }

        /// <summary>The computer to connect to. Default value is empty meaning that you are connecting to the local computer.</summary>
        [Parameter(Position = 1)]
        [Alias("PSComputerName")]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        /// <summary>Get subkeys from the specified path</summary>
        [Parameter()]
        public SwitchParameter Recurse { get; set; }

        /// <summary>Defines how many sub levels to get keys from if the Recurse parameter is used.</summary>
        [Parameter()]
        [ValidateRange(1, int.MaxValue)]
        public int Depth { get; set; } = int.MaxValue;

        /// <summary>Only get the registry key, not the properties</summary>
        [Parameter()]
        public SwitchParameter KeyOnly { get; set; }

        /// <summary>Specifies whether security checks are performed when opening registry keys and accessing their name/value pairs.</summary>
        [Parameter()]
        public RegistryKeyPermissionCheck KeyPermissionCheck { get; set; } = RegistryKeyPermissionCheck.ReadWriteSubTree;

        /// <summary>Specifies the access control rights used when opening each key.</summary>
        [Parameter()]
        public RegistryRights Rights { get; set; } = RegistryRights.EnumerateSubKeys | RegistryRights.QueryValues | RegistryRights.Notify | RegistryRights.SetValue;

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        /// <summary>Specifies optional behavior when retrieving name/value pairs from a registry key.</summary>
        [Parameter()]
        public RegistryValueOptions ValueOptions { get; set; } = RegistryValueOptions.None;
        #endregion


        protected override void BeginProcessing()
        {
            _GroupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(Path, this);
        }

        protected override void ProcessRecord()
        {
            foreach (string pcName in ComputerName)
            {
                foreach (RegistryHive hive in _GroupedRegKeysToProcess.Keys)
                {
                    RegistryKey baseKey;
                    try
                    {
                        WriteVerbose($"{pcName}: Opening base key {hive}");
                        baseKey = RegistryKey.OpenRemoteBaseKey(hive, pcName, View);
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToOpenBaseKey", Utility.GetErrorCategory(e), pcName));
                        continue;
                    }
                    bool shouldDisposeBaseKey = true;
                    foreach (string subKeyPath in _GroupedRegKeysToProcess[hive])
                    {
                        RegistryKey subKey;

                        if (subKeyPath == string.Empty)
                        {
                            WriteVerbose($"{pcName}: Sub key path is empty. Treating basekey as the subkey.");
                            subKey = baseKey;

                            shouldDisposeBaseKey = false;
                        }
                        else
                        {
                            try
                            {
                                WriteVerbose($"{pcName}: Opening sub key {subKeyPath}");
                                subKey = baseKey.OpenSubKey(subKeyPath, KeyPermissionCheck, Rights);
                                if (null == subKey)
                                {
                                    throw new ItemNotFoundException();
                                }
                            }
                            catch (Exception e) when (e is PipelineStoppedException == false)
                            {
                                WriteError(new ErrorRecord(e, "UnableToOpenSubKey", Utility.GetErrorCategory(e), subKeyPath));
                                continue;
                            }
                        }

                        if (Recurse)
                        {
                            int maxDepth= int.MaxValue;
                            int startDepth = subKey.Name.Length - subKey.Name.Replace($"{Utility._RegPathSeparator}", string.Empty).Length;
                            //Ensures maxDepth doesn't exceed int max value
                            if (int.MaxValue - Depth - startDepth >= 0)
                            {
                                maxDepth = Depth + startDepth;
                            }

                            List<RegistryKey> subKeysToCheck = new List<RegistryKey>() {subKey };
                            do
                            {
                                RegistryKey currentKey = subKeysToCheck[0];
                                subKeysToCheck.RemoveAt(0);

                                string[] foundSubKeyNames = new string[0];
                                int currentDepth = currentKey.Name.Length - currentKey.Name.Replace($"{Utility._RegPathSeparator}", string.Empty).Length;
                                if (currentDepth < maxDepth)
                                {
                                    try
                                    {
                                        foundSubKeyNames = currentKey.GetSubKeyNames();
                                    }
                                    catch (Exception e) when (e is PipelineStoppedException == false)
                                    {
                                        WriteError(new ErrorRecord(e, "UnableToGetSubKeyNames", Utility.GetErrorCategory(e), currentKey));
                                    }
                                    foreach (string keyName in foundSubKeyNames)
                                    {
                                        try
                                        {
                                            subKeysToCheck.Add(currentKey.OpenSubKey(keyName, KeyPermissionCheck, Rights));
                                        }
                                        catch (Exception e) when (e is PipelineStoppedException == false)
                                        {
                                            WriteError(new ErrorRecord(e, "UnableToOpenSubKey", Utility.GetErrorCategory(e), keyName));
                                        }
                                    }
                                }
                                Utility.WriteRegKeyToPipeline(this, currentKey, pcName, KeyOnly, ValueOptions, true);
                            } while (subKeysToCheck.Count > 0);
                        }
                        else
                        {
                            Utility.WriteRegKeyToPipeline(this, subKey, pcName, KeyOnly, ValueOptions, true);
                        }
                    }
                    if (shouldDisposeBaseKey)
                    {
                        baseKey.Dispose();
                    }
                }
            }
        }
    }
}