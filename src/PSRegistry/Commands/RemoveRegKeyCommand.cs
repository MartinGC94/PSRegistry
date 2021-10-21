using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Remove, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]

    public sealed class RemoveRegKeyCommand : Cmdlet
    {
        private const string whatIfText  = "Will delete key: \"{0}\"";
        private const string confirmText = "Delete key: \"{0}\"?";

        #region Parameters
        /// <summary>The path to the registry key. This only supports the full path (HKLM:\System or HKEY_LOCAL_MACHINE\System)</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("Name")]
        public string[] Path { get; set; }

        /// <summary>The computer to connect to. Default value is empty meaning that you are connecting to the local computer.</summary>
        [Parameter(Position = 1, ValueFromPipelineByPropertyName = true)]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        /// <summary>Specifies if keys with subkeys can be deleted.</summary>
        [Parameter()]
        public SwitchParameter Recurse { get; set; }

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;
        #endregion
        protected override void ProcessRecord()
        {
            Dictionary<RegistryHive, List<string>> groupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(Path, this);
            foreach (string pcName in ComputerName)
            {
                foreach (RegistryHive hive in groupedRegKeysToProcess.Keys)
                {
                    RegistryKey baseKey;
                    try
                    {
                        WriteVerbose($"{pcName}: Opening base key {hive}");
                        baseKey = RegistryKey.OpenRemoteBaseKey(hive, pcName, View);
                    }
                    catch (Exception e) when (e is PipelineStoppedException == false)
                    {
                        WriteError(new ErrorRecord(e, "UnableToOpenBaseKey", Utility.GetErrorCategory(e), pcName));
                        continue;
                    }
                    foreach (string subKeyPath in groupedRegKeysToProcess[hive])
                    {
                        if (subKeyPath == string.Empty)
                        {
                            var e = new ArgumentException($"The subkey path is empty. This command cannot delete registry hives such as {hive}.");
                            WriteError(new ErrorRecord(e, "UnableToDeleteBasekey", Utility.GetErrorCategory(e), baseKey));
                            continue;
                        }
                        string displayPath = $"{baseKey.Name}{Utility.regPathSeparator}{subKeyPath}";
                        string actualWhatIfText  = string.Format(whatIfText, displayPath);
                        string actualConfirmText = string.Format(confirmText, displayPath);

                        try
                        {
                            if (ShouldProcess(actualWhatIfText, actualConfirmText, Utility.confirmPrompt))
                            {
                                if (Recurse)
                                {
                                    baseKey.DeleteSubKeyTree(subKeyPath, true);
                                }
                                else
                                {
                                    baseKey.DeleteSubKey(subKeyPath, true);
                                }
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            WriteError(new ErrorRecord(e, "UnableToDeleteKey", Utility.GetErrorCategory(e), displayPath));
                        }
                    }
                    baseKey.Dispose();
                }
            }
        }
    }
}