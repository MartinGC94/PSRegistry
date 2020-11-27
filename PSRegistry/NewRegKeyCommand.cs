using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Win32;
using System.Security.AccessControl;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.New, "RegKey")]
    [OutputType(typeof(RegistryKey))]

    public sealed class NewRegKeyCommand : Cmdlet
    {
        private Dictionary<RegistryHive, List<string>> _GroupedRegKeysToProcess;

        #region Parameters
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] Path { get; set; }

        [Parameter(Position = 1)]
        [Alias("PSComputerName")]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        [Parameter()]
        public RegistryKeyPermissionCheck RegPermissionCheck { get; set; } = RegistryKeyPermissionCheck.Default;

        [Parameter()]
        public RegistryView RegView { get; set; } = RegistryView.Default;

        [Parameter()]
        public RegistryOptions RegOptions { get; set; } = RegistryOptions.None;

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
                        baseKey = RegistryKey.OpenRemoteBaseKey(hive, pcName, RegView);
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
                            var e = new ArgumentException();
                            WriteError(new ErrorRecord(e, "EmptySubKeyPath", Utility.GetErrorCategory(e), subKeyPath));
                            continue;
                        }
                        RegistryKey newKey;
                        try
                        {
                            if (ACL != null)
                            {
                                newKey = baseKey.CreateSubKey(subKeyPath, RegPermissionCheck, RegOptions, ACL);
                            }
                            else
                            {
                                newKey = baseKey.CreateSubKey(subKeyPath, RegPermissionCheck, RegOptions);
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            WriteError(new ErrorRecord(e, "UnableToCreateKey", Utility.GetErrorCategory(e), subKeyPath));
                            continue;
                        }

                        if (newKey == null)
                        {
                            var e = new ArgumentNullException();
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