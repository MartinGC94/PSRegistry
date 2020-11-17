using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace PSRegistry
{
    [Cmdlet(VerbsCommon.Rename, "RegKey")]
    [OutputType(typeof(RegistryKey))]

    public sealed class RenameRegKeyCommand : PSCmdlet
    {
        #region Parameters
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string[] Path { get; set; }

        [Parameter(Position = 1)]
        [Alias("PSComputerName")]
        public string[] ComputerName { get; set; } = new string[] { string.Empty };

        [Parameter()]
        public SwitchParameter Recurse { get; set; }

        [Parameter()]
        public int Depth { get; set; } = int.MaxValue;

        [Parameter()]
        public SwitchParameter KeyOnly { get; set; }

        [Parameter()]
        public SwitchParameter NoValueType { get; set; }

        [Parameter()]
        public RegistryKeyPermissionCheck PermissionCheck { get; set; } = RegistryKeyPermissionCheck.Default;

        [Parameter()]
        public RegistryRights RegistryRights { get; set; } = (RegistryRights.ReadKey & RegistryRights.WriteKey);

        [Parameter()]
        public RegistryView RegistryView { get; set; } = RegistryView.Default;
        #endregion
        protected override void ProcessRecord()
        {
        }
    }
}
