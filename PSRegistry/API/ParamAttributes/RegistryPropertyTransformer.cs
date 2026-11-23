using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace MartinGC94.PSRegistry.API.ParamAttributes
{
    public sealed class RegistryPropertyTransformer : ArgumentTransformationAttribute
    {
        public bool AssumeUwpType { get; set; } = false;

        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            object unwrappedInput = UnwrapObject(inputData);
            if (unwrappedInput is IDictionary inputDict)
            {
                return ProcessDictionary(inputDict);
            }

            if (unwrappedInput is IList inputList)
            {
                return ProcessList(inputList);
            }

            return inputData;
        }

        private RegistryProperty[] ProcessDictionary(IDictionary inputDict)
        {
            var result = new List<RegistryProperty>(inputDict.Count);
            foreach (DictionaryEntry entry in inputDict)
            {
                string propertyName = LanguagePrimitives.ConvertTo<string>(entry.Key);
                byte[] rawValue = RegistryProperty.InferRegistryTypeAndConvertValueToRawRegistryValue(
                    entry.Value,
                    assumeUwpType: AssumeUwpType,
                    out RegistryValueType valueKind);
                result.Add(new RegistryProperty(propertyName, rawValue, valueKind));
            }

            return result.ToArray();
        }

        private RegistryProperty[] ProcessList(IList list)
        {
            var result = new List<RegistryProperty>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                object unwrappedItem = UnwrapObject(list[i]);
                if (unwrappedItem is RegistryProperty regProperty)
                {
                    result.Add(regProperty);
                    continue;
                }

                if (unwrappedItem is IDictionary dict)
                {
                    bool propNameSpecified = false;
                    bool propValueSpecified = false;
                    bool propRawValueSpecified = false;
                    bool propValueTypeSpecified = false;
                    string propName = null;
                    object propValue = null;
                    RegistryValueType propValueType = default;

                    foreach (DictionaryEntry entry in dict)
                    {
                        string keyName = LanguagePrimitives.ConvertTo<string>(entry.Key);
                        if ("Name".Equals(keyName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (propNameSpecified)
                            {
                                throw new ArgumentException(string.Format(duplicateEntryMsg, "Name", i));
                            }

                            propName = LanguagePrimitives.ConvertTo<string>(entry.Value);
                            propNameSpecified = true;
                            continue;
                        }

                        if ("Value".Equals(keyName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (propValueSpecified)
                            {
                                throw new ArgumentException(string.Format(duplicateEntryMsg, "Value", i));
                            }

                            if (propRawValueSpecified)
                            {
                                throw new ArgumentException(string.Format(valueAndRawValueSpecifiedMsg, i));
                            }

                            propValue = entry.Value;
                            propValueSpecified = true;
                            continue;
                        }

                        if ("RawValue".Equals(keyName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (propRawValueSpecified)
                            {
                                throw new ArgumentException(string.Format(duplicateEntryMsg, "RawValue", i));
                            }

                            if (propValueSpecified)
                            {
                                throw new ArgumentException(string.Format(valueAndRawValueSpecifiedMsg, i));
                            }

                            propValue = entry.Value;
                            propRawValueSpecified = true;
                            continue;
                        }

                        if ("ValueKind".Equals(keyName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (propValueTypeSpecified)
                            {
                                throw new ArgumentException(string.Format(duplicateEntryMsg, "ValueKind", i));
                            }

                            propValueType = LanguagePrimitives.ConvertTo<RegistryValueType>(entry.Value);
                            propValueTypeSpecified = true;
                            continue;
                        }

                        throw new ArgumentException($"Unknown property '{keyName}'. Allowed values are: 'Name', 'ValueKind', and either 'Value' or 'RawValue'. Check hashtable at index {i}");
                    }

                    if (!propNameSpecified)
                    {
                        throw new ArgumentException($"'Name' value is missing. Check hashtable at index {i}");
                    }

                    if (!propValueSpecified && !propRawValueSpecified)
                    {
                        throw new ArgumentException($"A value is missing. Specify either 'Value' or 'RawValue'. Check hashtable at index {i}");
                    }

                    if (propRawValueSpecified)
                    {
                        byte[] rawValue = LanguagePrimitives.ConvertTo<byte[]>(propValue);
                        result.Add(new RegistryProperty(propName, rawValue, propValueType));
                        continue;
                    }

                    byte[] actualValue = propValueTypeSpecified
                        ? RegistryProperty.ConvertValueToRawRegistryValue(propValue, propValueType)
                        : RegistryProperty.InferRegistryTypeAndConvertValueToRawRegistryValue(propValue, AssumeUwpType, out propValueType);
                    result.Add(new RegistryProperty(propName, actualValue, propValueType));
                    continue;
                }

                throw new ArgumentException($"Cannot convert value {unwrappedItem} to a {nameof(RegistryProperty)}");
            }

            return result.ToArray();
        }

        private const string duplicateEntryMsg = "Duplicate '{0}' entry for hashtable at index {1}.";
        private const string valueAndRawValueSpecifiedMsg = "'Value' and 'RawValue' cannot be specified at the same time. Check hashtable at index {0}.";

        private object UnwrapObject(object obj)
        {
            return obj is PSObject psObj ? psObj.BaseObject : obj;
        }
    }
}