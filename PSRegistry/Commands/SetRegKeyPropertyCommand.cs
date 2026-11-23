using MartinGC94.PSRegistry.API;
using MartinGC94.PSRegistry.API.ParamAttributes;
using Microsoft.Win32;
using System;
using System.Management.Automation;
using System.Security.AccessControl;
using MartinGC94.PSRegistry.Native;
using System.ComponentModel;

namespace MartinGC94.PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.Set, "RegKeyProperty", DefaultParameterSetName = "FromName")]
    [Alias("Add-RegKeyProperty")]
    public sealed class SetRegKeyPropertyCommand : PSCmdlet
    {
        /// <summary>The reg key(s) to add properties to.</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegistryKeyTransformer(RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.SetValue, isSingleKey: false)]
        [ValidateNotNullOrEmpty]
        public RegistryKey[] Key { get; set; }

        /// <summary>The property name to add.</summary>
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "FromName")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "FromNameRaw")]
        [AllowEmptyString]
        public string[] Name { get; set; }

        /// <summary>The property value to add.</summary>
        [Parameter(Position = 2, ParameterSetName = "FromName")]
        public object Value { get; set; }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "FromNameRaw")]
        public byte[] RawValue
        {
            get => inputValueAsRaw;
            set => inputValueAsRaw = value;
        }

        /// <summary>The registry value Kind/Type to treat the value as</summary>
        [Parameter(Position = 3, ParameterSetName = "FromName")]
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "FromNameRaw")]
        public RegistryValueType ValueKind
        {
            get => inputValueKind;
            set => inputValueKind = value;
        }

        /// <summary>The property to add.</summary>
        [Parameter(Mandatory = true, ParameterSetName = "FromObject")]
        [RegistryPropertyTransformer()]
        public RegistryProperty[] Property
        {
            get => inputRegProperties;
            set => inputRegProperties = value;
        }

        [Parameter(Mandatory = true, ParameterSetName = "FromObjectUWP")]
        [RegistryPropertyTransformer(AssumeUwpType = true)]
        public RegistryProperty[] UwpProperty
        {
            get => inputRegProperties;
            set => inputRegProperties = value;
        }

        /// <summary>Switch to not dispose the registry key(s) when done.</summary>
        [Parameter()]
        [Alias("DontCloseKey", "NoDispose")]
        public SwitchParameter DontDisposeKey { get; set; }

        RegistryProperty[] inputRegProperties;
        byte[] inputValueAsRaw;
        RegistryValueType inputValueKind;

        protected override void BeginProcessing()
        {
            if (MyInvocation.BoundParameters.ContainsKey(nameof(Value)))
            {
                try
                {
                    if (MyInvocation.BoundParameters.ContainsKey(nameof(ValueKind)))
                    {
                        inputValueAsRaw = RegistryProperty.ConvertValueToRawRegistryValue(Value, ValueKind);
                    }
                    else
                    {
                        inputValueAsRaw = RegistryProperty.InferRegistryTypeAndConvertValueToRawRegistryValue(Value, assumeUwpType: false, out inputValueKind);
                    }
                }
                catch (Exception e)
                {
                    ThrowTerminatingError(e.NewErrorRecord("ConvertInputValueError", Value));
                }
            }
        }

        protected override void ProcessRecord()
        {
            foreach (RegistryKey regKey in Key)
            {
                try
                {
                    if (Name != null)
                    {
                        SetRegKeyValueFromRawValue(regKey);
                    }
                    else
                    {
                        SetRegKeyValueFromProperties(regKey);
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

        private void SetRegKeyValueFromRawValue(RegistryKey regKey)
        {
            foreach (string propertyName in Name)
            {
                SetRegKeyValue(regKey, propertyName, inputValueAsRaw, inputValueKind);
            }
        }

        private void SetRegKeyValueFromProperties(RegistryKey regKey)
        {
            foreach (RegistryProperty property in inputRegProperties)
            {
                SetRegKeyValue(regKey, property.Name, property.RawValue, property.ValueKind);
            }
        }

        private void SetRegKeyValue(RegistryKey regKey, string propertyName, byte[] value, RegistryValueType valueKind)
        {
            int length = value is null ? 0 : value.Length;
            int returnCode = NativeMethods.RegSetKeyValueW(regKey.Handle, null, propertyName, valueKind, value, length);
            if (returnCode != ERROR_SUCCESS)
            {
                var nativeError = new Win32Exception(returnCode);
                var exception = new ApiException($"Failed to set property {propertyName} in {regKey.Name} due to error: {nativeError.Message}", nativeError);
                WriteError(exception.NewErrorRecord("SetValueError", propertyName));
            }
        }

        private const int ERROR_SUCCESS = 0;
    }
}