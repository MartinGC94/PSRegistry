using MartinGC94.PSRegistry.API;
using MartinGC94.PSRegistry.Native;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Management.Automation;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.Rename, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class RenameRegKeyCommand : Cmdlet
    {
        /// <summary>The path to the key to rename.</summary>
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        [Alias("Name")]
        public string Path { get; set; }

        /// <summary>The new name for the key.</summary>
        [Parameter(Position = 1, Mandatory = true)]
        public string NewName { get; set; }

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        protected override void EndProcessing()
        {
            RegistryHive hiveToOpen;
            string subkeyPath;
            try
            {
                hiveToOpen = Util.GetRegistryHiveFromPath(Path, out subkeyPath);
            }
            catch (ArgumentException e)
            {
                WriteError(e.NewErrorRecord("InvalidPath", Path));
                return;
            }

            RegistryKey baseKey;
            try
            {
                baseKey = RegistryKey.OpenBaseKey(hiveToOpen, View);
            }
            catch (Exception e)
            {
                WriteError(new ApiException($"Failed to open basekey {hiveToOpen} due to error: {e.Message}", e).NewErrorRecord("BasekeyOpenError", hiveToOpen));
                return;
            }

            string keyPathToRename = $"{baseKey.Name}\\{subkeyPath}";
            using (baseKey)
            {
                if (ShouldProcess($"{keyPathToRename} -> {NewName}"))
                {
                    int returnCode = NativeMethods.RegRenameKey(baseKey.Handle, subkeyPath, NewName);
                    if (returnCode != ERROR_SUCCESS)
                    {
                        var nativeError = new Win32Exception(returnCode);
                        WriteError(new ApiException(
                            $"Failed to rename key {keyPathToRename} due to error: {nativeError.Message}", nativeError)
                            .NewErrorRecord("RenameKeyError", keyPathToRename));
                    }
                }
            }
        }
        private const int ERROR_SUCCESS = 0;
    }
}
