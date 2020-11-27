using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Win32;


namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Move, "RegKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(RegistryKey))]

    public sealed class MoveRegKeyCommand : Cmdlet
    {
        private Dictionary<RegistryHive, List<string>> _GroupedRegKeysToProcess;

        #region Parameters
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        [RegKeyTransform]
        [ValidateNotNullOrEmpty]
        [Alias("Source", "Path")]
        public RegistryKey RegKey { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        public string[] Destination { get; set; }

        [Parameter(Position = 2)]
        [Alias("PSComputerName")]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        [Parameter()]
        public RegistryView View { get; set; } = RegistryView.Default;
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