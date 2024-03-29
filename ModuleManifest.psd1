@{
    RootModule             = if ($PSEdition -eq "Core") {"netstandard2.0\PSRegistry.dll"} else {"net462\PSRegistry.dll"}
    ModuleVersion          = {0}
    CompatiblePSEditions   = @("Core", "Desktop")
    GUID                   = 'ea7fa35e-49f6-4594-a7e4-344984da0523'
    Author                 = 'MartinGC94'
    CompanyName            = 'Unknown'
    Copyright              = '(c) 2021 MartinGC94. All rights reserved.'
    Description            = 'Module for managing registry keys and properties.'
    PowerShellVersion      = '5.1'
    DotNetFrameworkVersion = '4.6.2'
    FormatsToProcess       = @('Registry.format.ps1xml')
    FunctionsToExport      = @()
    CmdletsToExport        = @({1})
    VariablesToExport      = @()
    AliasesToExport        = @()
    DscResourcesToExport   = @()
    FileList               = @({2})
    PrivateData            = @{
        PSData = @{
             Tags         = @("Registry", "Reg")
             ProjectUri   = 'https://github.com/MartinGC94/PSRegistry'
             ReleaseNotes = @'
{3}
'@
        }
    }
}