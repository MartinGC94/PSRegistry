using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System;


namespace PSRegistry
{
    public class RegistryProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public RegistryValueKind Type { get; set; }
    }
    public class RegKeyTransform : ArgumentTransformationAttribute
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
                path = Array.ConvertAll(inputData as object[],item => item.ToString());
            }

            Cmdlet commandToRun = new GetRegKeyCommand()
            {
                Path = path,
                KeyOnly = true
            };
            var commandOutput = commandToRun.Invoke().GetEnumerator();

            var resultData = new List<PSObject>();
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
    public class RegPropertyTransform : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            if (inputData is RegistryProperty)
            {
                return inputData;
            }
            Hashtable inputDataAsHashtable = inputData as Hashtable;
            var result = new List<RegistryProperty>();
            foreach (var key in inputDataAsHashtable.Keys)
            {
                result.Add( new RegistryProperty() { Name = key.ToString(), Type = RegistryValueKind.Unknown, Value = inputDataAsHashtable[key] });
            }
            return result.ToArray();
        }
    }
}