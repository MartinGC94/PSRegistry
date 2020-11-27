using System;
using System.Management.Automation;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Remove, "RegKey",SupportsShouldProcess = true,ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(RegistryKey))]

    public sealed class RemoveRegKeyCommand : Cmdlet
    {
        private const char   _PathSeparator      = '\\';
        private const string _WhatIfText         = "Will delete key: \"{0}\"";
        private const string _ConfirmText        = "Delete key: \"{0}\"?";

        #region Parameters
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Name")]
        public string[] Path { get; set; }

        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        [Parameter()]
        public SwitchParameter Recurse { get; set; }
        #endregion
        protected override void ProcessRecord()
        {
            foreach (string PathString in Path)
            {
                string trimmedPathString = PathString.Trim(_PathSeparator);
                int indexOfLastSeparator = trimmedPathString.LastIndexOf(_PathSeparator);
                if (indexOfLastSeparator < 0)
                {
                    //String looks like a basekey due to a lack of separators. Basekeys cannot be deleted.
                    var e = new NotSupportedException();
                    WriteError(new ErrorRecord(e, "UnableToDeleteBasekey", Utility.GetErrorCategory(e), PathString));
                    continue;
                }
                string[] parentPath = new string[] { PathString.Substring(0, indexOfLastSeparator) };
                string childPath = trimmedPathString.Substring(indexOfLastSeparator+1);

                var commandToRun = new GetRegKeyCommand()
                {
                    Path = parentPath,
                    ComputerName = ComputerName,
                    Rights = (RegistryRights.Delete | RegistryRights.EnumerateSubKeys | RegistryRights.QueryValues | RegistryRights.SetValue),
                    PermissionCheck= RegistryKeyPermissionCheck.ReadWriteSubTree,
                    KeyOnly = true
                };
                var commandOutput= commandToRun.Invoke().GetEnumerator();
                while (commandOutput.MoveNext())
                {
                    RegistryKey key = (commandOutput.Current as PSObject).BaseObject as RegistryKey;
                    try
                    {
                        string actualWhatIfText  = string.Format(_WhatIfText,  $"{key.Name}\\{childPath}");
                        string actualConfirmText = string.Format(_ConfirmText, $"{key.Name}\\{childPath}");

                        if (ShouldProcess(actualWhatIfText, actualConfirmText, "Confirm"))
                        {
                            if (Recurse)
                            {
                                key.DeleteSubKeyTree(childPath, true);
                            }
                            else
                            {
                                key.DeleteSubKey(childPath, true);
                            }
                        }
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToDeleteKey", Utility.GetErrorCategory(e), PathString));
                    }
                    finally
                    {
                        key.Dispose();
                    }
                }
            }
        }
    }
}