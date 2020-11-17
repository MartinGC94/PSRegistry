using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

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
            if (inputData is RegistryKey)
            {
                return inputData;
            }

            string inputDataAsString = inputData as string;
            string[] targetComputer = new string[] {string.Empty};
            string[] path = new string[] {inputDataAsString};

            if (inputDataAsString.StartsWith("\\\\"))
            {
                targetComputer[0] = inputDataAsString.Split('\\')[2];
                path[0] = inputDataAsString.Substring(targetComputer.Length + 3);
            }
            Cmdlet commandToRun = new GetRegKeyCommand() { Path = path, ComputerName =targetComputer, KeyOnly = true};
            var commandResult = commandToRun.Invoke<RegistryKey>();
            return commandResult.ToArray();
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