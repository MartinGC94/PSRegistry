using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Management.Automation;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Rename, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]

    public sealed class RenameRegKeyCommand : Cmdlet
    {
        private const string whatIfText = "Will rename \"{0}\" key to \"{1}\"";
        private const string confirmText = "Rename \"{0}\" key to \"{1}\"?";

        #region Parameters
        /// <summary>The path to the key to rename.</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [Alias("Name")]
        public string[] Path { get; set; }

        /// <summary>The new name for the key.</summary>
        [Parameter(Position = 1, Mandatory = true)]
        public string NewName { get; set; }

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;
        #endregion

        protected override void ProcessRecord()
        {
            var groupedKeys = Utility.GroupKeyPathsByBaseKey(Path, this);
            foreach (RegistryHive hive in groupedKeys.Keys)
            {
                RegistryKey baseKey;
                try
                {
                    baseKey = RegistryKey.OpenBaseKey(hive, View);
                }
                catch (Exception e) when (e is PipelineStoppedException == false)
                {
                    WriteError(new ErrorRecord(e, "UnableToOpenBaseKey", Utility.GetErrorCategory(e), hive));
                    continue;
                }
                foreach (string subKeyPath in groupedKeys[hive])
                {
                    string displaySourcePath = $"{baseKey.Name}{Utility.regPathSeparator}{subKeyPath}";
                    string displayDestPath = $"{displaySourcePath.Substring(0, displaySourcePath.LastIndexOf(Utility.regPathSeparator))}{Utility.regPathSeparator}{NewName}";

                    string actualWhatIfText = string.Format(whatIfText, displaySourcePath, displayDestPath);
                    string actualConfirmText = string.Format(confirmText, displaySourcePath, displayDestPath);

                    if (ShouldProcess(actualWhatIfText, actualConfirmText, Utility.confirmPrompt))
                    {
                        try
                        {
                            int returnCode = NativeMethods.RegRenameKey(baseKey.Handle, subKeyPath, NewName);
                            if (returnCode != 0)
                            {
                                throw new Win32Exception(returnCode);
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            WriteError(new ErrorRecord(e, "UnableToRenameKey", Utility.GetErrorCategory(e), displaySourcePath));
                        }
                    }
                }
                baseKey.Dispose();
            }
        }
    }
}