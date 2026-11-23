using System;
using System.Management.Automation;
using System.Text;

namespace MartinGC94.PSRegistry.API
{
    public sealed class RegistryProperty
    {
        public string Name { get; set; }
        public object Value
        {
            get => GetValueFromRawValue();
            set => RawValue = ConvertValueToRawRegistryValue(value, ValueKind);
        }
        public RegistryValueType ValueKind { get; }
        public DateTime TimeStamp => GetTimeStamp();
        public byte[] RawValue { get; set; }
        // Value types between 0 and 11 are classic registry types. Others are assumed to function like the known UWP registry types.
        private bool IsUwpProperty => (uint)ValueKind > 11;

        public RegistryProperty(string propName, byte[] rawValue, RegistryValueType valueKind)
        {
            Name = propName;
            ValueKind = valueKind;
            RawValue = rawValue;
        }

        public RegistryProperty(string propName, object value, RegistryValueType valueKind)
        {
            Name = propName;
            ValueKind = valueKind;
            RawValue = ConvertValueToRawRegistryValue(value, valueKind);
        }

        public override string ToString()
        {
            return $"{ValueKind}: {Value}";
        }

        private DateTime GetTimeStamp()
        {
            if (!IsUwpProperty || RawValue is null || RawValue.Length < 8)
            {
                return DateTime.MinValue;
            }

            long fileTime = BitConverter.ToInt64(RawValue, RawValue.Length - 8);
            return DateTime.FromFileTime(fileTime);
        }

        private object GetValueFromRawValue()
        {
            if (RawValue is null || RawValue.Length == 0)
            {
                return null;
            }

            switch (ValueKind)
            {
                case RegistryValueType.String:
                case RegistryValueType.ExpandString:
                    return Encoding.Unicode.GetString(RawValue).TrimEnd('\0');

                case RegistryValueType.DWord:
                case RegistryValueType.UwpUint32:
                    return BitConverter.ToUInt32(RawValue, 0);

                case RegistryValueType.MultiString:
                    return Encoding.Unicode.GetString(RawValue, 0, RawValue.Length).TrimEnd('\0').Split('\0');

                case RegistryValueType.QWord:
                    return BitConverter.ToUInt64(RawValue, 0);

                case RegistryValueType.UwpInt32:
                    return BitConverter.ToInt32(RawValue, 0);

                case RegistryValueType.UwpBool:
                    return BitConverter.ToBoolean(RawValue, 0);

                case RegistryValueType.UwpString:
                    return Encoding.Unicode.GetString(RawValue, 0, RawValue.Length - 8).TrimEnd('\0');

                case RegistryValueType.UwpDateTimeOffset:
                    return DateTimeOffset.FromFileTime(BitConverter.ToInt64(RawValue, 0));

                default:
                    return RawValue;
            }
        }

        internal static byte[] InferRegistryTypeAndConvertValueToRawRegistryValue(object value, bool assumeUwpType, out RegistryValueType valueKind)
        {
            object unwrappedValue = value is PSObject wrappedValue
                ? wrappedValue.BaseObject
                : value;

            switch (unwrappedValue)
            {
                case string _:
                    valueKind = assumeUwpType ? RegistryValueType.UwpString : RegistryValueType.String;
                    break;

                case string[] _:
                    valueKind = RegistryValueType.MultiString;
                    break;

                case byte[] _:
                    valueKind = RegistryValueType.Binary;
                    break;

                case int _:
                    valueKind = assumeUwpType ? RegistryValueType.UwpInt32 : RegistryValueType.DWord;
                    break;

                case uint _:
                    valueKind = assumeUwpType ? RegistryValueType.UwpUint32 : RegistryValueType.DWord;
                    break;

                case ulong _:
                case long _:
                    valueKind = RegistryValueType.QWord;
                    break;

                case bool _:
                case SwitchParameter _:
                    valueKind = assumeUwpType ? RegistryValueType.UwpBool : RegistryValueType.DWord;
                    break;

                case DateTimeOffset _:
                    if (assumeUwpType)
                    {
                        valueKind = RegistryValueType.UwpDateTimeOffset;
                        break;
                    }
                    goto default;

                case object[] psArray:
                    if (psArray.Length > 0)
                    {
                        object firstElement = psArray[0];
                        if (firstElement is PSObject wrappedValue2)
                        {
                            firstElement = wrappedValue2.BaseObject;
                        }

                        if (firstElement is byte && LanguagePrimitives.TryConvertTo(unwrappedValue, out byte[] byteArray))
                        {
                            valueKind = RegistryValueType.Binary;
                            return ConvertValueToRawRegistryValue(byteArray, valueKind);
                        }
                        
                        if (firstElement is string && LanguagePrimitives.TryConvertTo(unwrappedValue, out string[] stringArray))
                        {
                            valueKind = RegistryValueType.MultiString;
                            return ConvertValueToRawRegistryValue(stringArray, valueKind);
                        }

                        goto default;
                    }
                    else
                    {
                        goto default;
                    }

                default:
                    throw new ArgumentException($"Unable to find a compatible registry type for value '{value}'.");
            }

            return ConvertValueToRawRegistryValue(unwrappedValue, valueKind);
        }

        internal static byte[] ConvertValueToRawRegistryValue(object value, RegistryValueType targetType)
        {
            object unwrappedValue = value is PSObject wrappedValue
                ? wrappedValue.BaseObject
                : value;

            if (unwrappedValue is byte[] unwrappedByteArray
                && (targetType == RegistryValueType.Binary
                || targetType == RegistryValueType.None))
            {
                return unwrappedByteArray;
            }

            try
            {
                switch (targetType)
                {
                    case RegistryValueType.None:
                    case RegistryValueType.Binary:
                        return LanguagePrimitives.ConvertTo<byte[]>(unwrappedValue);

                    case RegistryValueType.String:
                    case RegistryValueType.ExpandString:
                        string stringValue = LanguagePrimitives.ConvertTo<string>(unwrappedValue) + '\0';
                        return Encoding.Unicode.GetBytes(stringValue);

                    case RegistryValueType.DWord:
                        uint numberAsUint = LanguagePrimitives.ConvertTo<uint>(unwrappedValue);
                        return BitConverter.GetBytes(numberAsUint);

                    case RegistryValueType.MultiString:
                        string[] stringArray = LanguagePrimitives.ConvertTo<string[]>(unwrappedValue);
                        string joinedString = string.Join("\0", stringArray) + "\0\0";
                        return Encoding.Unicode.GetBytes(joinedString);

                    case RegistryValueType.QWord:
                        ulong numberAsUlong = LanguagePrimitives.ConvertTo<ulong>(unwrappedValue);
                        return BitConverter.GetBytes(numberAsUlong);

                    case RegistryValueType.UwpInt32:
                        int numberAsInt = LanguagePrimitives.ConvertTo<int>(unwrappedValue);
                        return AppendTimestamp(BitConverter.GetBytes(numberAsInt));

                    case RegistryValueType.UwpUint32:
                        uint numberAsUint2 = LanguagePrimitives.ConvertTo<uint>(unwrappedValue);
                        return AppendTimestamp(BitConverter.GetBytes(numberAsUint2));

                    case RegistryValueType.UwpBool:
                        bool valueAsBool = LanguagePrimitives.ConvertTo<bool>(unwrappedValue);
                        return AppendTimestamp(BitConverter.GetBytes(valueAsBool));

                    case RegistryValueType.UwpString:
                        string valueAsString = LanguagePrimitives.ConvertTo<string>(unwrappedValue) + '\0';
                        return AppendTimestamp(Encoding.Unicode.GetBytes(valueAsString));

                    case RegistryValueType.UwpDateTimeOffset:
                        DateTimeOffset dateTimeOffset = LanguagePrimitives.ConvertTo<DateTimeOffset>(unwrappedValue);
                        return AppendTimestamp(BitConverter.GetBytes(dateTimeOffset.ToFileTime()));

                    default:
                        throw new ArgumentException($"Unknown registry type: {targetType}. Convert the value to a byte array manually and assign it as a Raw value.");
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Cannot convert value '{value}' to a usable type for '{targetType}' due to error: {e.Message}", e);
            }
        }

        private static byte[] AppendTimestamp(byte[] value)
        {
            byte[] timeStamp = BitConverter.GetBytes(DateTime.Now.ToFileTime());
            var output = new byte[value.Length + timeStamp.Length];
            Array.Copy(value, output, value.Length);
            Array.Copy(timeStamp, 0, output, value.Length, timeStamp.Length);
            return output;
        }
    }
}