using System;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Win32;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Add, "RegKeyProperty",DefaultParameterSetName = "FromName")]
    [OutputType(typeof(RegistryKey))]

    public sealed class AddRegKeyPropertyCommand : Cmdlet
    {
        private RegistryProperty[] _RegistryPropertiesToSet;

        #region Parameters
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyTransform]
        [ValidateNotNullOrEmpty]
        public RegistryKey[] RegKey { get; set; }

        [Parameter(Mandatory = true, ParameterSetName = "FromObject")]
        [RegPropertyTransform]
        public RegistryProperty[] Property { get; set; }

        [Parameter(Mandatory = true,ParameterSetName = "FromName")]
        public string[] Name { get; set; }

        [Parameter(ParameterSetName = "FromName")]
        public object Value { get; set; }

        [Parameter(ParameterSetName = "FromName")]
        public RegistryValueKind Type { get; set; } = RegistryValueKind.Unknown;
        #endregion

        protected override void BeginProcessing()
        {
            if (Name != null)
            {
                var someList = new List<RegistryProperty>();
                foreach (string item in Name)
                {
                    someList.Add(new RegistryProperty() { Name = item, Value = Value, Type = Type });
                }
                _RegistryPropertiesToSet = someList.ToArray();
            }
            else
            {
                _RegistryPropertiesToSet = Property;
            }
        }
        protected override void ProcessRecord()
        {
            foreach (RegistryKey key in RegKey)
            {
                foreach (RegistryProperty property in _RegistryPropertiesToSet)
                {
                    try
                    {
                        object actualValue = property.Value;
                        if (null == actualValue)
                        {
                            switch (property.Type)
                            {
                                case RegistryValueKind.Binary:
                                    actualValue = new byte[0];
                                    break;
                                case RegistryValueKind.DWord:
                                    actualValue = 0;
                                    break;
                                case RegistryValueKind.QWord:
                                    actualValue = 0;
                                    break;
                                default:
                                    actualValue = string.Empty;
                                    break;
                            }
                        }

                        key.SetValue(property.Name, actualValue, property.Type);
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToSetValue", Utility.GetErrorCategory(e), property));
                    }
                }
            }
        }
    }
}