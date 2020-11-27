using System;
using System.Management.Automation;
using Microsoft.Win32;
using System.ComponentModel;

namespace PSRegistry
{
    [Cmdlet(VerbsData.Dismount, "RegHive")]
    [OutputType(typeof(RegistryKey))]

    public sealed class DismountRegHiveCommand : PSCmdlet
    {
        #region Parameters
        [Parameter(Position = 1, Mandatory = true)]
        [ValidateSet("LocalMachine", "Users")]
        public RegistryHive MountHive { get; set; }

        [Parameter(Position = 2, Mandatory = true)]
        public string SubKeyName { get; set; }

        [Parameter(Position = 3)]
        public string ComputerName { get; set; } = string.Empty;
        #endregion
        protected override void ProcessRecord()
        {
            bool privWasPreviouslyEnabled = false;
            RegistryKey baseKey = null;
            try
            {
                NativeMethods.RtlAdjustPrivilege((ulong)Utility.WindowsPrivileges.SeRestorePrivilege, true, false, out privWasPreviouslyEnabled);

                baseKey = RegistryKey.OpenRemoteBaseKey(MountHive, ComputerName);

                int returnCode = NativeMethods.RegUnLoadKey(baseKey.Handle.DangerousGetHandle(), SubKeyName);
                if (returnCode != 0)
                {
                    throw new Win32Exception(returnCode);
                }
            }
            catch (Exception e) when (e is PipelineStoppedException == false)
            {
                WriteError(new ErrorRecord(e, "UnableToDismountHive", Utility.GetErrorCategory(e), SubKeyName));
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