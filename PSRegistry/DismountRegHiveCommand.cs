using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;

namespace PSRegistry
{
    [Cmdlet(VerbsData.Dismount, "RegHive")]

    public sealed class DismountRegHiveCommand : Cmdlet
    {
        private Dictionary<RegistryHive, List<string>> _GroupedRegKeysToProcess;
        private bool _PrivWasPreviouslyEnabled;

        #region Parameters
        /// <summary>The path to the registry key to dismount.</summary>
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] Path { get; set; }

        /// <summary>The computer where the key(s) should be dismounted from.</summary>
        [Parameter(Position = 1)]
        public string[] ComputerName { get; set; } = new string[1] { string.Empty };

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;
        #endregion

        protected override void BeginProcessing()
        {
            _GroupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(Path, this);
            NativeMethods.RtlAdjustPrivilege((ulong)Utility.WindowsPrivileges.SeRestorePrivilege, true, false, out _PrivWasPreviouslyEnabled);
        }

        protected override void ProcessRecord()
        {
            foreach (string pcName in ComputerName)
            {
                foreach (RegistryHive hive in _GroupedRegKeysToProcess.Keys)
                {
                    RegistryKey baseKey = null;
                    try
                    {
                        baseKey = RegistryKey.OpenRemoteBaseKey(hive, pcName,View);
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToOpenBaseKey", Utility.GetErrorCategory(e), pcName));
                        continue;
                    }
                    foreach (string subKeyPath in _GroupedRegKeysToProcess[hive])
                    {
                        try
                        {
                            int returnCode = NativeMethods.RegUnLoadKey(baseKey.Handle.DangerousGetHandle(), subKeyPath);
                            if (returnCode != 0)
                            {
                                throw new Win32Exception(returnCode);
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            WriteError(new ErrorRecord(e, "UnableToDismountKey", Utility.GetErrorCategory(e), subKeyPath));
                        }
                    }
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