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
        private Dictionary<RegistryHive, List<string>> groupedRegKeysToProcess;

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
            groupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(Path, this);
        }

        protected override void ProcessRecord()
        {
            foreach (string pcName in ComputerName)
            {
                foreach (RegistryHive hive in groupedRegKeysToProcess.Keys)
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
                    foreach (string subKeyPath in groupedRegKeysToProcess[hive])
                    {
                        if (subKeyPath == string.Empty)
                        {
                            var e = new ArgumentException($"The subkey path is empty. This command cannot create registry hives such as {hive}.");
                            WriteError(new ErrorRecord(e, "EmptySubKeyPath", Utility.GetErrorCategory(e), subKeyPath));
                            continue;
                        }
                        RegistryKey newKey;
                        try
                        {
                            if (ACL is null)
                            {
                                newKey = baseKey.CreateSubKey(subKeyPath, KeyPermissionCheck, Options);
                            }
                            else
                            {
                                newKey = baseKey.CreateSubKey(subKeyPath, KeyPermissionCheck, Options, ACL);
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

                        Utility.WriteRegKeyToPipeline(this, newKey, pcName, keyOnly:true, RegistryValueOptions.None, includeValueKind:false, asPsObject:true);
                    }
                    baseKey.Dispose();
                }
            }
        }
    }
}