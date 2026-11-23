using MartinGC94.PSRegistry.API;
using Microsoft.Win32;
using System;
using System.Management.Automation;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.Remove, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public sealed class RemoveRegKeyCommand : Cmdlet
    {
        /// <summary>The path to the registry key. This only supports the full path (HKLM:\System or HKEY_LOCAL_MACHINE\System)</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Name")]
        public string[] Path { get; set; }

        /// <summary>The computer to connect to. Default value is empty meaning that you are connecting to the local computer.</summary>
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        /// <summary>Specifies if keys with subkeys can be deleted.</summary>
        [Parameter()]
        public SwitchParameter Recurse { get; set; }

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        protected override void ProcessRecord()
        {
            foreach (string pcName in ComputerName)
            {
                foreach (string pathItem in Path)
                {
                    RegistryHive hiveToOpen;
                    string subkeyPath;
                    try
                    {
                        hiveToOpen = Util.GetRegistryHiveFromPath(pathItem, out subkeyPath);
                    }
                    catch (ArgumentException e)
                    {
                        WriteError(e.NewErrorRecord("InvalidPath", pathItem));
                        continue;
                    }

                    if (string.IsNullOrEmpty(subkeyPath))
                    {
                        var e = new ArgumentException($"The subkey path is empty. This command cannot delete registry hives such as {hiveToOpen}.");
                        WriteError(e.NewErrorRecord("EmptySubkeyPath", pathItem));
                        continue;
                    }

                    RegistryKey baseKey = Util.OpenBaseKey(this, hiveToOpen, pcName, View);
                    if (baseKey is null)
                    {
                        continue;
                    }

                    try
                    {
                        if (ShouldProcess(pathItem))
                        {
                            if (Recurse)
                            {
                                baseKey.DeleteSubKeyTree(subkeyPath, throwOnMissingSubKey: true);
                            }
                            else
                            {
                                baseKey.DeleteSubKey(subkeyPath, throwOnMissingSubKey: true);
                            }
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        ErrorRecord error = new ApiException($"The registry key {pathItem} contains subkeys and cannot be deleted without the Recurse option.", e)
                            .NewErrorRecord("KeyHasChildren", pathItem);
                        error.SetRecommendedAction("Rerun the command with the -Recurse parameter set.");
                        WriteError(error);
                    }
                    catch (Exception e)
                    {
                        WriteError(new ApiException($"Failed to delete '{pathItem}' due to error: {e.Message}", e).NewErrorRecord("DeleteKeyError", pathItem));
                    }
                    finally
                    {
                        baseKey.Dispose();
                    }
                }
            }
        }
    }
}