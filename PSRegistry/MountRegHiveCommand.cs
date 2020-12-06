using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;

namespace PSRegistry
{
    [Cmdlet(VerbsData.Mount, "RegHive")]

    public sealed class MountRegHiveCommand : PSCmdlet
    {
        private Dictionary<RegistryHive, List<string>> _GroupedRegKeysToProcess;
        private bool _PrivWasPreviouslyEnabled;

        #region Parameters
        /// <summary>The path to the registry database file that should be mounted.</summary>
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("FilePath")]
        public string Path { get; set; }

        /// <summary>The registry path where the hive should be mounted.</summary>
        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("MountPath")]
        public string DestinationPath { get; set; }

        /// <summary>The computer where the registry hive is mounted.</summary>
        [Parameter(Position = 2)]
        public string ComputerName { get; set; } = string.Empty;

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;
        #endregion

        protected override void BeginProcessing()
        {
            _GroupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(new string[1] {DestinationPath}, this);
            NativeMethods.RtlAdjustPrivilege((ulong)Utility.WindowsPrivileges.SeRestorePrivilege, true, false, out _PrivWasPreviouslyEnabled);
        }

        protected override void ProcessRecord()
        {
            RegistryKey baseKey = null;
            string filePath = null;
            try
            {
                //Get file path for the file to mount, and the registry hive + subkey to mount it to.
                filePath = GetUnresolvedProviderPathFromPSPath(Path);
                Dictionary<RegistryHive, List<string>>.KeyCollection.Enumerator enumerator = _GroupedRegKeysToProcess.Keys.GetEnumerator();
                enumerator.MoveNext();
                RegistryHive mountHive = enumerator.Current;
                string mountSubKey=_GroupedRegKeysToProcess[mountHive][0];


                baseKey = RegistryKey.OpenRemoteBaseKey(mountHive, ComputerName,View);

                int returnCode = NativeMethods.RegLoadKey(baseKey.Handle.DangerousGetHandle(), mountSubKey, filePath);
                if (returnCode != 0)
                {
                    throw new Win32Exception(returnCode);
                }
            }
            catch (Exception e) when (e is PipelineStoppedException == false)
            {
                WriteError(new ErrorRecord(e, "UnableToMountHive", Utility.GetErrorCategory(e), filePath));
            }
            finally
            {
                if (null != baseKey)
                {
                    baseKey.Dispose();
                }
            }
        }
        protected override void EndProcessing()
        {
            if (!_PrivWasPreviouslyEnabled)
            {
                NativeMethods.RtlAdjustPrivilege((ulong)Utility.WindowsPrivileges.SeRestorePrivilege, false, false, out _PrivWasPreviouslyEnabled);
            }
        }
    }
}