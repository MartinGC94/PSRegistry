using Microsoft.Win32;
using System;
using System.Management.Automation;

namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Remove, "RegKeyProperty", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]

    public sealed class RemoveRegKeyPropertyCommand : Cmdlet
    {
        private const string whatIfText = "Will delete \"{0}\" property from \"{1}\"";
        private const string confirmText = "Delete \"{0}\" property from \"{1}\"?";

        #region Parameters
        /// <summary>The registry key(s) where the properties should be deleted from..</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyAddRemovePropertyTransform]
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
        #endregion
        protected override void ProcessRecord()
        {
            foreach (RegistryKey regKey in Key)
            {
                foreach (string name in PropertyName)
                {
                    try
                    {
                        if (ShouldProcess(string.Format(whatIfText, name, regKey.Name), string.Format(confirmText, name, regKey.Name), Utility.confirmPrompt))
                        {
                            regKey.DeleteValue(name, true);
                        }
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToRemoveValue", Utility.GetErrorCategory(e), name));
                    }
                }
                if (!DontDisposeKey)
                {
                    regKey.Dispose();
                }
            }
        }
    }
}