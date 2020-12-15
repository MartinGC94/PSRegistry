using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;


namespace PSRegistry
{
    public sealed class RegistryProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public RegistryValueKind ValueKind { get; set; }
    }
    internal sealed class RegKeyTransform : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {

            if (inputData is RegistryKey || inputData is PSObject && (inputData as PSObject).BaseObject is RegistryKey)
            {
                return inputData;
            }

            string[] path;
            if (inputData is string)
            {
                path = new string[] { inputData as string };
            }
            else
            {
                path = Array.ConvertAll(inputData as object[], item => item.ToString());
            }

            Cmdlet commandToRun = new GetRegKeyCommand()
            {
                Path = path,
                KeyOnly = true
            };
            IEnumerator commandOutput = commandToRun.Invoke().GetEnumerator();

            List<PSObject> resultData = new List<PSObject>();
            while (commandOutput.MoveNext())
            {
                resultData.Add(commandOutput.Current as PSObject);
            }
            if (resultData.Count == 1)
            {
                return resultData[0];
            }
            else
            {
                return resultData.ToArray();
            }
        }
    }
    internal sealed class RegPropertyTransform : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            if (inputData is RegistryProperty)
            {
                return inputData;
            }
            Hashtable inputDataAsHashtable = inputData as Hashtable;
            List<RegistryProperty> result = new List<RegistryProperty>();
            foreach (object key in inputDataAsHashtable.Keys)
            {
                result.Add(new RegistryProperty() { Name = key.ToString(), ValueKind = RegistryValueKind.Unknown, Value = inputDataAsHashtable[key] });
            }
            return result.ToArray();
        }
    }
}