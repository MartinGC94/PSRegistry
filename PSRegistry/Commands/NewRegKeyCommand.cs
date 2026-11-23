using MartinGC94.PSRegistry.API;
using Microsoft.Win32;
using System;
using System.Management.Automation;
using System.Security.AccessControl;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.New, "RegKey")]
    [OutputType(typeof(RegistryKey))]
    public sealed class NewRegKeyCommand : Cmdlet
    {
        /// <summary>The path to the registry key. This only supports the full path (HKLM:\System or HKEY_LOCAL_MACHINE\System)</summary>
        [Parameter(Position = 0, Mandatory = true)]
        public string[] Path { get; set; }

        /// <summary>The computer to connect to. Default value is empty meaning that you are connecting to the local computer.</summary>
        [Parameter(Position = 1)]
        [Alias("PSComputerName")]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        /// <summary>Specifies whether security checks are performed when opening registry keys and accessing their name/value pairs.</summary>
        [Parameter()]
        public RegistryKeyPermissionCheck KeyPermissionCheck { get; set; } = RegistryKeyPermissionCheck.Default;

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        /// <summary>Specifies options such as if the key is volatile or not.</summary>
        [Parameter()]
        public RegistryOptions Options { get; set; } = RegistryOptions.None;

        /// <summary>Specifies a custom ACL to apply to the new key.</summary>
        [Parameter()]
        public RegistrySecurity ACL { get; set; }

        protected override void EndProcessing()
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
                        var e = new ArgumentException($"The subkey path is empty. This command cannot create registry hives such as {hiveToOpen}.");
                        WriteError(e.NewErrorRecord("EmptySubkeyPath", pathItem));
                        continue;
                    }

                    RegistryKey baseKey = Util.OpenBaseKey(this, hiveToOpen, pcName, View);
                    if (baseKey is null)
                    {
                        continue;
                    }

                    RegistryKey newKey;
                    using (baseKey)
                    {
                        try
                        {
                            newKey = ACL is null
                            ? baseKey.CreateSubKey(subkeyPath, KeyPermissionCheck, Options)
                            : baseKey.CreateSubKey(subkeyPath, KeyPermissionCheck, Options, ACL);
                        }
                        catch (Exception e)
                        {
                            WriteError(new ApiException($"Failed to create key '{pathItem}' due to error: {e.Message}", e).NewErrorRecord("KeyCreateError", pathItem));
                            continue;
                        }
                    }

                    if (newKey is null)
                    {
                        var nullError = new NullReferenceException();
                        WriteError(new ApiException($"Failed to create key {pathItem} due to an unspecified error.", nullError).NewErrorRecord("NewKeyWasNull", pathItem));
                        continue;
                    }

                    PSObject wrappedKey = newKey.WrapRegistryKey(pcName);
                    WriteObject(wrappedKey);
                }
            }
        }
    }
}