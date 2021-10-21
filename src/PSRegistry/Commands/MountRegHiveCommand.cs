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
        private Dictionary<RegistryHive, List<string>> groupedRegKeysToProcess;
        private bool privWasPreviouslyEnabled;

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
            groupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(new string[1] {DestinationPath}, this);
            NativeMethods.RtlAdjustPrivilege(WindowsPrivileges.SeRestorePrivilege, Enable:true, CurrentThread:false, out privWasPreviouslyEnabled);
        }

        protected override void ProcessRecord()
        {
            RegistryKey baseKey = null;
            string filePath = null;
            try
            {
                //Get file path for the file to mount, and the registry hive + subkey to mount it to.
                filePath = GetUnresolvedProviderPathFromPSPath(Path);
                RegistryHive mountHive;
                using (var enumerator = groupedRegKeysToProcess.Keys.GetEnumerator())
                {
                    _ = enumerator.MoveNext();
                    mountHive = enumerator.Current;
                }
                string mountSubKey = groupedRegKeysToProcess[mountHive][0];

                baseKey = RegistryKey.OpenRemoteBaseKey(mountHive, ComputerName, View);
                int returnCode = NativeMethods.RegLoadKey(baseKey.Handle, mountSubKey, filePath);
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
            if (!privWasPreviouslyEnabled)
            {
                NativeMethods.RtlAdjustPrivilege(WindowsPrivileges.SeRestorePrivilege, Enable:false, CurrentThread:false, out privWasPreviouslyEnabled);
            }
        }
    }
}