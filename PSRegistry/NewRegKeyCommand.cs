using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Security.AccessControl;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.New, "RegKey")]
    [OutputType(typeof(RegistryKey))]

    public sealed class NewRegKeyCommand : Cmdlet
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

        /// <summary>Specifies whether security checks are performed when opening registry keys and accessing their name/value pairs.</summary>
        [Parameter()]
        public RegistryKeyPermissionCheck KeyPermissionCheck { get; set; } = RegistryKeyPermissionCheck.Default;

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        /// <summary>Specifies options such as if the key is volatile or not.</summary>
        [Parameter()]
        public RegistryOptions Options { get; set; } = RegistryOptions.None;

        /// <summary>Specifies a custom ACL to apply to the new key.</summary>
        [Parameter()]
        public RegistrySecurity ACL { get; set; }
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
                    foreach (string subKeyPath in _GroupedRegKeysToProcess[hive])
                    {
                        if (subKeyPath == string.Empty)
                        {
                            ArgumentException e = new ArgumentException();
                            WriteError(new ErrorRecord(e, "EmptySubKeyPath", Utility.GetErrorCategory(e), subKeyPath));
                            continue;
                        }
                        RegistryKey newKey;
                        try
                        {
                            if (ACL != null)
                            {
                                newKey = baseKey.CreateSubKey(subKeyPath, KeyPermissionCheck, Options, ACL);
                            }
                            else
                            {
                                newKey = baseKey.CreateSubKey(subKeyPath, KeyPermissionCheck, Options);
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            WriteError(new ErrorRecord(e, "UnableToCreateKey", Utility.GetErrorCategory(e), subKeyPath));
                            continue;
                        }

                        if (newKey == null)
                        {
                            ArgumentNullException e = new ArgumentNullException();
                            WriteError(new ErrorRecord(e, "NewKeyWasNull", Utility.GetErrorCategory(e), subKeyPath));
                            continue;
                        }

                        Utility.WriteRegKeyToPipeline(this, newKey, pcName, true, RegistryValueOptions.None, false);
                    }
                    baseKey.Dispose();
                }
            }
        }
    }
}