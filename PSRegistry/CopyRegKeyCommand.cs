using Microsoft.Win32;
using System.Collections.Generic;
using System.Management.Automation;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Copy, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]

    public sealed class CopyRegKeyCommand : Cmdlet
    {
        private Dictionary<RegistryHive, List<string>> _GroupedRegKeysToProcess;

        #region Parameters
        /// <summary>The key to copy.</summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyTransform]
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
        #endregion

        protected override void BeginProcessing()
        {
            _GroupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(Destination, this);
        }

        protected override void ProcessRecord()
        {
            Utility.CopyRegistryCommand(this, _GroupedRegKeysToProcess);
        }
    }
}