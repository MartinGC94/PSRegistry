using System;
using System.Management.Automation;
using Microsoft.Win32;

namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Remove, "RegKeyProperty",SupportsShouldProcess = true,ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(RegistryKey))]

    public sealed class RemoveRegKeyPropertyCommand : Cmdlet
    {
        private const string _WhatIfText = "Will delete \"{0}\" property from \"{1}\"";
        private const string _ConfirmText = "Delete \"{0}\" property from \"{1}\"?";

        #region Parameters
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyTransform]
        [ValidateNotNullOrEmpty]
        public RegistryKey[] RegKey { get; set; }

        [Parameter(Mandatory = true)]
        [Alias("Name")]
        public string[] PropertyName { get; set; }
        #endregion
        protected override void ProcessRecord()
        {
            foreach (var key in RegKey)
            {
                foreach (var name in PropertyName)
                {
                    try
                    {
                        if (ShouldProcess(string.Format(_WhatIfText,name,key.Name),string.Format(_ConfirmText,name,key.Name),"Confirm"))
                        {
                            key.DeleteValue(name, true);
                        }
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToRemoveValue", Utility.GetErrorCategory(e), name));
                    }
                }
            }
        }
    }
}