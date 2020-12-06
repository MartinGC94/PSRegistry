using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Add, "RegKeyProperty", DefaultParameterSetName = _FromNameParamSet)]

    public sealed class AddRegKeyPropertyCommand : Cmdlet
    {
        private const string _FromNameParamSet   = "FromName";
        private const string _FromObjectParamSet = "FromObject";
        private RegistryProperty[] _RegistryPropertiesToSet;

        #region Parameters
        /// <summary>The reg key(s) to add properties to.</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyTransform]
        [ValidateNotNullOrEmpty]
        public RegistryKey[] Key { get; set; }

        /// <summary>The property name to add.</summary>
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = _FromNameParamSet)]
        [AllowEmptyString]
        public string[] Name { get; set; }

        /// <summary>The property value to add.</summary>
        [Parameter(Position = 2, ParameterSetName = _FromNameParamSet)]
        public object Value { get; set; }

        /// <summary>The registry value Kind/Type to treat the value as</summary>
        [Parameter(Position = 3, ParameterSetName = _FromNameParamSet)]
        public RegistryValueKind ValueKind { get; set; } = RegistryValueKind.Unknown;

        /// <summary>The property to add.</summary>
        [Parameter(Mandatory = true, ParameterSetName = _FromObjectParamSet)]
        [RegPropertyTransform]
        public RegistryProperty[] Property { get; set; }

        /// <summary>Switch to not dispose the registry key(s) when done.</summary>
        [Parameter()]
        [Alias("DontCloseKey", "NoDispose")]
        public SwitchParameter DontDisposeKey { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            if (Name != null)
            {
                List<RegistryProperty> propertyList = new List<RegistryProperty>();
                foreach (string item in Name)
                {
                    propertyList.Add(new RegistryProperty() { Name = item, Value = Value, Type = ValueKind });
                }
                _RegistryPropertiesToSet = propertyList.ToArray();
            }
            else
            {
                _RegistryPropertiesToSet = Property;
            }
        }
        protected override void ProcessRecord()
        {
            foreach (RegistryKey regKey in Key)
            {
                foreach (RegistryProperty property in _RegistryPropertiesToSet)
                {
                    try
                    {
                        object valueToSet = property.Value;

                        if (property.Type == RegistryValueKind.String || property.Type == RegistryValueKind.ExpandString)
                        {
                            if (null == valueToSet)
                            {
                                valueToSet = string.Empty;
                            }
                            else
                            {
                                valueToSet = valueToSet.ToString();
                            }
                        }
                        else if (property.Type == RegistryValueKind.Binary)
                        {
                            if (null == valueToSet)
                            {
                                valueToSet = new byte[0];
                            }
                            else
                            {
                                if (valueToSet.GetType() == typeof(string))
                                {
                                    valueToSet = Encoding.Unicode.GetBytes(valueToSet as string);
                                }
                                else if (valueToSet.GetType() == typeof(object[]))
                                {
                                    valueToSet = Array.ConvertAll(valueToSet as object[], item => (byte)(int)item);
                                }
                                else if (valueToSet.GetType() == typeof(int))
                                {
                                    valueToSet = new byte[1] { (byte)(int)valueToSet };
                                }
                            }
                        }
                        else if (property.Type == RegistryValueKind.DWord || property.Type == RegistryValueKind.QWord)
                        {
                            if (null == valueToSet)
                            {
                                valueToSet = 0;
                            }
                        }
                        else if (property.Type == RegistryValueKind.MultiString)
                        {
                            if (null == valueToSet)
                            {
                                valueToSet = new string[0];
                            }
                            else if (valueToSet.GetType() == typeof(object[]))
                            {
                                valueToSet = Array.ConvertAll(valueToSet as object[], item => item.ToString());
                            }
                            else if (valueToSet.GetType() != typeof(string[]))
                            {
                                valueToSet = new string[1] { valueToSet.ToString() };
                            }
                        }
                        else
                        {
                            if (null == valueToSet)
                            {
                                valueToSet = string.Empty;
                            }
                            if (valueToSet.GetType() == typeof(object[]))
                            {
                                switch ((valueToSet as object[])[0])
                                {
                                    case int _:
                                        valueToSet = valueToSet = Array.ConvertAll(valueToSet as object[], item => (byte)(int)item);
                                        break;
                                    case string _:
                                        valueToSet = valueToSet = Array.ConvertAll(valueToSet as object[], item => item.ToString());
                                        break;
                                }
                            }
                        }

                        regKey.SetValue(property.Name, valueToSet, property.Type);
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToSetValue", Utility.GetErrorCategory(e), property));
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