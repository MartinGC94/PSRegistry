using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Security.AccessControl;

namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Copy, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]

    public sealed class CopyRegKeyCommand : Cmdlet
    {
        private Dictionary<RegistryHive, List<string>> groupedRegKeysToProcess;
        private const RegistryRights fallbackDestinationKeyRights = RegistryRights.EnumerateSubKeys | RegistryRights.QueryValues | RegistryRights.SetValue;
        private const string whatIfText = "Will overwrite \"{0}\" with data from \"{1}\"";
        private const string confirmText = "Overwrite \"{0}\" with data from \"{1}\"?";
        private const string whatIfTextNewKey = "Will create \"{0}\" and copy data from \"{1}\"";
        private const string confirmTextNewKey = "Create \"{0}\" and copy data from \"{1}\"?";

        #region Parameters
        /// <summary>The key to copy.</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyCopyTransform]
        [ValidateNotNullOrEmpty]
        [Alias("Source", "Path")]
        public RegistryKey Key { get; set; }

        /// <summary>The destination(s) to copy the key to.</summary>
        [Parameter(Position = 1, Mandatory = true)]
        public string[] Destination { get; set; }

        /// <summary>The destination computer(s) to copy the key to.</summary>
        [Parameter(Position = 2)]
        [Alias("PSComputerName")]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        /// <summary>The registry view to use.</summary>
        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;

        /// <summary>Switch to not dispose the registry key(s) when done.</summary>
        [Parameter()]
        [Alias("DontCloseKey", "NoDispose")]
        public SwitchParameter DontDisposeKey { get; set; }

        /// <summary>The permissions used to open the destination key</summary>
        [Parameter()]
        public RegistryRights DestinationKeyRights { get; set; }
        #endregion

        protected override void BeginProcessing()
        {
            groupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(Destination, this);
        }

        protected override void ProcessRecord()
        {
            RegistryRights keyRights = DestinationKeyRights;
            if (DestinationKeyRights == 0)
            {
                try
                {
                    keyRights = Utility.GetRegRightsFromKey(Key);
                }
                catch (Exception e) when (e is PipelineStoppedException == false)
                {
                    WriteWarning($"Unable to get original RegistryRights from source key handle. Using default rights: {fallbackDestinationKeyRights} for the destination key.");
                }
                if (keyRights == 0)
                {
                    keyRights = fallbackDestinationKeyRights;
                }
            }
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
                        string displayDestPath = $"{baseKey.Name}{Utility.regPathSeparator}{subKeyPath}";
                        string formattedWhatIf;
                        string formattedConfirm;
                        RegistryKey destinationKey = null;

                        try
                        {
                            destinationKey = baseKey.OpenSubKey(subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree, keyRights);

                            if (destinationKey is null)
                            {
                                formattedWhatIf = string.Format(whatIfTextNewKey, displayDestPath, Key.Name);
                                formattedConfirm = string.Format(confirmTextNewKey, displayDestPath, Key.Name);
                            }
                            else
                            {
                                formattedWhatIf = string.Format(whatIfText, displayDestPath, Key.Name);
                                formattedConfirm = string.Format(confirmText, displayDestPath, Key.Name);
                            }

                            if (ShouldProcess(formattedWhatIf, formattedConfirm, Utility.confirmPrompt))
                            {
                                if (destinationKey is null)
                                {
                                    destinationKey = baseKey.CreateSubKey(subKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                                }
                                int returnCode = NativeMethods.RegCopyTree(Key.Handle, string.Empty, destinationKey.Handle);
                                if (returnCode != 0)
                                {
                                    throw new Win32Exception(returnCode);
                                }
                            }
                        }
                        catch (Exception e) when (e is PipelineStoppedException == false)
                        {
                            WriteError(new ErrorRecord(e, "UnableToCopyKey", Utility.GetErrorCategory(e), Key));
                        }
                        finally
                        {
                            if (null != destinationKey)
                            {
                                destinationKey.Dispose();
                            }
                        }
                    }
                    baseKey.Dispose();
                }
            }
            if (!DontDisposeKey)
            {
                Key.Dispose();
            }
        }
    }
}