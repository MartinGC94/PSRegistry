using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Add, "RegKeyProperty", DefaultParameterSetName = fromNameParamSet)]

    public sealed class AddRegKeyPropertyCommand : Cmdlet
    {
        private const string fromNameParamSet   = "FromName";
        private const string fromObjectParamSet = "FromObject";
        private List<RegistryProperty> registryPropertiesToSet;

        #region Parameters
        /// <summary>The reg key(s) to add properties to.</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyAddRemovePropertyTransform]
        [ValidateNotNullOrEmpty]
        public RegistryKey[] Key { get; set; }

        /// <summary>The property name to add.</summary>
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = fromNameParamSet)]
        [AllowEmptyString]
        public string[] Name { get; set; }

        /// <summary>The property value to add.</summary>
        [Parameter(Position = 2, ParameterSetName = fromNameParamSet)]
        public object Value { get; set; }

        /// <summary>The registry value Kind/Type to treat the value as</summary>
        [Parameter(Position = 3, ParameterSetName = fromNameParamSet)]
        public RegistryValueKind ValueKind { get; set; } = RegistryValueKind.Unknown;

        /// <summary>The property to add.</summary>
        [Parameter(Mandatory = true, ParameterSetName = fromObjectParamSet)]
        [RegPropertyTransform]
        public RegistryProperty[] Property { get; set; }

        /// <summary>Switch to not dispose the registry key(s) when done.</summary>
        [Parameter()]
        [Alias("DontCloseKey", "NoDispose")]
        public SwitchParameter DontDisposeKey { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            List<RegistryProperty> propertiesToConvert;
            if (Name != null)
            {
                propertiesToConvert = new List<RegistryProperty>(Name.Length);
                foreach (string item in Name)
                {
                    propertiesToConvert.Add(new RegistryProperty() { Name = item, Value = Value, ValueKind = ValueKind });
                }
            }
            else
            {
                propertiesToConvert = new List<RegistryProperty>(Property);
            }
            for (int i = propertiesToConvert.Count - 1; i >= 0; i--)
            {
                try
                {
                    propertiesToConvert[i] = Utility.ConvertRegistryPropertyData(propertiesToConvert[i]);
                }
                catch (Exception e) when (e is PipelineStoppedException == false)
                {
                    WriteError(new ErrorRecord(e, "UnableToConvertValue", Utility.GetErrorCategory(e), propertiesToConvert[i]));
                    propertiesToConvert.RemoveAt(i);
                }
            }
            registryPropertiesToSet = propertiesToConvert;
        }
        protected override void ProcessRecord()
        {
            foreach (RegistryKey regKey in Key)
            {
                foreach (RegistryProperty property in registryPropertiesToSet)
                {
                    try
                    {
                        regKey.SetValue(property.Name, property.Value, property.ValueKind);
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToSetValue", Utility.GetErrorCategory(e), property));
                        if (e is ObjectDisposedException)
                        {
                            break;
                        }
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