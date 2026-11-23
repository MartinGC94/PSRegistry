using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Management.Automation;
using System.Text;

namespace PSRegistry.Commands
{
    [Cmdlet(VerbsCommon.Get, "RegKeyUwpProperty")]
    public sealed class GetRegKeyUwpPropertyCommand : Cmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyAddRemovePropertyTransform]
        public RegistryKey Key { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        [AllowEmptyString]
        public string[] Property { get; set; }

        [Parameter]
        public SwitchParameter Raw { get; set; }

        private byte[] outputBuffer = new byte[1024];

        protected override void ProcessRecord()
        {
            if (Key.Name.StartsWith("HKEY_PERFORMANCE_DATA", StringComparison.OrdinalIgnoreCase))
            {
                WriteError(new ErrorRecord(
                    new ArgumentException("HKEY_PERFORMANCE_DATA keys cannot be processed by this command."),
                    "InvalidRegKey",
                    ErrorCategory.InvalidArgument,
                    null));

                return;
            }

            foreach (string propName in Property)
            {
                GetRegKeyValue(propName);
                Array.Clear(outputBuffer, 0, outputBuffer.Length);
            }
        }

        private void GetRegKeyValue(string propertyName)
        {
            int returnCode;
            PsRegistryValueKind propertyType;
            int bufferSize = outputBuffer.Length;
            while (true)
            {
                returnCode = NativeMethods.RegGetValueW(Key.Handle, null, propertyName, RRF_RT_ANY, out propertyType, outputBuffer, ref bufferSize);
                if (returnCode != ERROR_MORE_DATA)
                {
                    break;
                }

                outputBuffer = new byte[bufferSize];
            }

            if (returnCode != ERROR_SUCCESS)
            {
                Win32Exception e = new Win32Exception(returnCode);
                WriteError(new ErrorRecord(
                    e,
                    "GetPropValueError",
                    Utility.GetErrorCategory(e),
                    propertyName));
                
                return;
            }

            if (Raw)
            {
                var outputArray = new byte[bufferSize];
                Array.Copy(outputBuffer, outputArray, bufferSize);
                var output = new RawUwpRegistryProperty(propertyName, outputArray, propertyType);
                WriteObject(output);
                return;
            }

            object value;
            switch (propertyType)
            {
                case PsRegistryValueKind.UwpInt32:
                    value = BitConverter.ToInt32(outputBuffer, 0);
                    break;
                case PsRegistryValueKind.UwpUint32:
                    value = BitConverter.ToUInt32(outputBuffer, 0);
                    break;
                case PsRegistryValueKind.UwpBool:
                    value = BitConverter.ToBoolean(outputBuffer, 0);
                    break;
                case PsRegistryValueKind.UwpString:
                    value = Encoding.Unicode.GetString(outputBuffer, 0, bufferSize - 8);
                    break;
                default:
                    byte[] tempArray = new byte[bufferSize];
                    Array.Copy(outputBuffer, tempArray, bufferSize - 8);
                    value = tempArray;
                    break;
            }

            long fileTime = BitConverter.ToInt64(outputBuffer, bufferSize - 8);
            WriteObject(new UwpRegistryProperty(propertyName, value, DateTime.FromFileTime(fileTime), propertyType));
        }

        private const int ERROR_SUCCESS = 0;
        private const int ERROR_MORE_DATA = 234;
        private const int RRF_RT_ANY = 0x0000ffff;
    }
}
