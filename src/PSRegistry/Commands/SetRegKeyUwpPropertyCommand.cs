using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Management.Automation;

namespace PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.Set, "RegKeyUwpProperty")]
    public class SetRegKeyUwpPropertyCommand : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyAddRemovePropertyTransform]
        public RegistryKey Key { get; set; }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = "FromName")]
        [AllowEmptyString]
        public string[] Name { get; set; }

        [Parameter(Position = 2, ParameterSetName = "FromName")]
        public object Value { get; set; }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "FromName")]
        public PsRegistryValueKind ValueKind { get; set; }

        [Parameter(ParameterSetName = "FromName")]
        public SwitchParameter Raw { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "FromProperty")]
        [RegPropertyTransform]
        public UwpRegistryProperty[] Property { get; set; }

        [Parameter()]
        [Alias("DontCloseKey", "NoDispose")]
        public SwitchParameter DontDisposeKey { get; set; }

        protected override void ProcessRecord()
        {
            if (Name != null)
            {
                ProcessNameInput();
            }
            else
            {
                ProcessPropertyInput();
            }

            if (!DontDisposeKey)
            {
                Key.Dispose();
            }
        }

        private void ProcessNameInput()
        {
            byte[] valueToSet;
            if (Raw)
            {
                object unwrappedValue = Value is PSObject psObject
                    ? psObject.BaseObject
                    : Value;

                valueToSet = unwrappedValue as byte[];
                if (valueToSet is null)
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("When -Raw is specified, -Value must contain a byte array."),
                        "MissingByteArray",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }
            }
            else
            {
                try
                {
                    valueToSet = Utility.GetUwpValueBytesFromObject(Value, DateTime.Now, out _);
                }
                catch (ArgumentException e)
                {
                    WriteError(new ErrorRecord(
                        e,
                        "InvalidValue",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }
            }

            foreach (string propName in Name)
            {
                SetRegKeyValue(propName, valueToSet, ValueKind);
            }
        }

        private void ProcessPropertyInput()
        {
            foreach (UwpRegistryProperty propToSet in Property)
            {
                byte[] valueToSet;
                try
                {
                    valueToSet = Utility.GetUwpValueBytesFromObject(propToSet.Value, propToSet.TimeStamp, out _);
                }
                catch (ArgumentException e)
                {
                    WriteError(new ErrorRecord(
                        e,
                        "InvalidObjectInput",
                        ErrorCategory.InvalidArgument,
                        propToSet));
                    continue;
                }

                SetRegKeyValue(propToSet.Name, valueToSet, propToSet.ValueKind);
            }
        }

        private void SetRegKeyValue(string propertyName, byte[] value, PsRegistryValueKind valueKind)
        {
            int returnCode = NativeMethods.RegSetKeyValueW(Key.Handle, null, propertyName, valueKind, value, value.Length);
            if (returnCode != 0)
            {
                var e = new Win32Exception(returnCode);
                WriteError(new ErrorRecord(
                    e,
                    "SetValueError",
                    Utility.GetErrorCategory(e),
                    propertyName));
            }
        }
    }
}
