using MartinGC94.PSRegistry.API;
using MartinGC94.PSRegistry.Native;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Management.Automation;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsData.Mount, "RegHive")]
    public sealed class MountRegHiveCommand : PSCmdlet
    {
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

        protected override void EndProcessing()
        {
            RegistryHive mountHive;
            string subKey;
            try
            {
                mountHive = Util.GetRegistryHiveFromPath(DestinationPath, out subKey);
            }
            catch (ArgumentException e)
            {
                WriteError(e.NewErrorRecord("InvalidMountPath", DestinationPath));
                return;
            }

            string filePath;
            try
            {
                filePath = GetUnresolvedProviderPathFromPSPath(Path);
            }
            catch (Exception e)
            {
                WriteError(e.NewErrorRecord("InvalidFilePath", Path));
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
                    returnCode = NativeMethods.RegLoadKeyW(baseKey.Handle, subKey, filePath);
                    if (returnCode != ERROR_SUCCESS)
                    {
                        var nativeException = new Win32Exception(returnCode);
                        WriteError(new ApiException($"Failed to mount hive due to error: {nativeException.Message}", nativeException).NewErrorRecord("HiveMountError", filePath));
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
