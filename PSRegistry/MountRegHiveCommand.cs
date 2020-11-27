using System;
using System.Management.Automation;
using Microsoft.Win32;
using System.ComponentModel;

namespace PSRegistry
{
    [Cmdlet(VerbsData.Mount, "RegHive")]

    public sealed class MountRegHiveCommand : PSCmdlet
    {
        #region Parameters
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("FilePath")]
        public string Path { get; set; }

        [Parameter(Position = 1,Mandatory = true)]
        [ValidateSet("LocalMachine", "Users")]
        public RegistryHive MountHive { get; set; }

        [Parameter(Position = 2,Mandatory = true)]
        public string SubKeyName { get; set; }

        [Parameter(Position = 3)]
        public string ComputerName { get; set; } = string.Empty;
        #endregion
        protected override void ProcessRecord()
        {
            bool privWasPreviouslyEnabled=false;
            string filePath = GetUnresolvedProviderPathFromPSPath(Path);
            RegistryKey baseKey = null;
            try
            {
                NativeMethods.RtlAdjustPrivilege((ulong)Utility.WindowsPrivileges.SeRestorePrivilege, true, false, out privWasPreviouslyEnabled);

                baseKey = RegistryKey.OpenRemoteBaseKey(MountHive, ComputerName);

                int returnCode = NativeMethods.RegLoadKey(baseKey.Handle.DangerousGetHandle(), SubKeyName, filePath);
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
                if (!privWasPreviouslyEnabled)
                {
                    NativeMethods.RtlAdjustPrivilege((ulong)Utility.WindowsPrivileges.SeRestorePrivilege, false, false, out privWasPreviouslyEnabled);
                }
            }
        }
    }
}