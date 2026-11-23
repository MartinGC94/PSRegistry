using MartinGC94.PSRegistry.API;
using MartinGC94.PSRegistry.API.ParamAttributes;
using Microsoft.Win32;
using System;
using System.Management.Automation;
using System.Security.AccessControl;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.Remove, "RegKeyProperty", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public sealed class RemoveRegKeyPropertyCommand : Cmdlet
    {
        /// <summary>The registry key(s) where the properties should be deleted from..</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegistryKeyTransformer(RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.SetValue, isSingleKey: false)]
        [ValidateNotNullOrEmpty]
        public RegistryKey[] Key { get; set; }

        /// <summary>Name(s) of the properties to delete.</summary>
        [Parameter(Position = 1, Mandatory = true)]
        [Alias("Name")]
        [AllowEmptyString]
        public string[] PropertyName { get; set; }

        /// <summary>Switch to not dispose the registry key(s) when done.</summary>
        [Parameter()]
        [Alias("DontCloseKey", "NoDispose")]
        public SwitchParameter DontDisposeKey { get; set; }

        protected override void ProcessRecord()
        {
            foreach (RegistryKey regKey in Key)
            {
                try
                {
                    foreach (string name in PropertyName)
                    {
                        try
                        {
                            if (ShouldProcess($"{regKey.Name} : {name}"))
                            {
                                regKey.DeleteValue(name, throwOnMissingValue: true);
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            WriteError(new ApiException($"Failed to delete property {name} due to error: {e.Message}", e).NewErrorRecord("DeletePropertyError", name));
                        }
                    }
                }
                finally
                {
                    if (!DontDisposeKey)
                    {
                        regKey.Dispose();
                    }
                }
            }
        }
    }
}