using Microsoft.Win32;
using System.Collections.Generic;
using System.Management.Automation;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Move, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    //Move-RegKey works but I'm not completely happy with the way it works (Removes key even if source/dest is the same, can fail on removing key after copying it.).
    //So make it internal for now.
    internal sealed class MoveRegKeyCommand : Cmdlet
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
        public string Destination { get; set; }

        /// <summary>The destination computer(s) to copy the key to.</summary>
        [Parameter(Position = 2)]
        [Alias("PSComputerName")]
        public string ComputerName { get; set; } = string.Empty;

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
            _GroupedRegKeysToProcess = Utility.GroupKeyPathsByBaseKey(new string[1] { Destination }, this);
        }

        protected override void ProcessRecord()
        {
            Utility.CopyRegistryCommand(this, _GroupedRegKeysToProcess);
        }
    }
}