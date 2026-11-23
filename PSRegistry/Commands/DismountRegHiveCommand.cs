using MartinGC94.PSRegistry.API;
using MartinGC94.PSRegistry.Native;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Management.Automation;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsData.Dismount, "RegHive")]
    public sealed class DismountRegHiveCommand : Cmdlet
    {
        /// <summary>The path to the registry key to dismount.</summary>
        [Parameter(Position = 0, Mandatory = true)]
        public string Path { get; set; }

        /// <summary>The computer where the key should be dismounted from.</summary>
        [Parameter(Position = 1)]
        public string ComputerName { get; set; } = string.Empty;

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        protected override void EndProcessing()
        {
            RegistryHive mountHive;
            string subKey;
            try
            {
                mountHive = Util.GetRegistryHiveFromPath(Path, out subKey);
            }
            catch (ArgumentException e)
            {
                WriteError(e.NewErrorRecord("InvalidMountPath", Path));
                return;
            }

            RegistryKey baseKey = Util.OpenBaseKey(this, mountHive, ComputerName, View);
            if (baseKey is null)
            {
                return;
            }

            using (baseKey)
            {
                int returnCode = Util.ToggleRestorePrivilege(this, true, out bool wasPreviouslyEnabled);
                if (returnCode != ERROR_SUCCESS)
                {
                    return;
                }

                try
                {
                    returnCode = NativeMethods.RegUnLoadKeyW(baseKey.Handle, subKey);
                    if (returnCode != ERROR_SUCCESS)
                    {
                        string keyPath = $"{baseKey.Name}\\{subKey}";
                        var nativeException = new Win32Exception(returnCode);
                        WriteError(new ApiException($"Failed to unmount hive at {keyPath} due to error: {nativeException.Message}", nativeException)
                            .NewErrorRecord("HiveDismountError", keyPath));
                    }
                }
                finally
                {
                    if (!wasPreviouslyEnabled)
                    {
                        _ = Util.ToggleRestorePrivilege(this, false, out _);
                    }
                }
            }
        }

        private const int ERROR_SUCCESS = 0;
    }
}