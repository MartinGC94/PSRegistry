@{
    ModuleName="PSRegistry"
    ManifestData=@{
        Guid="ea7fa35e-49f6-4594-a7e4-344984da0523"
        Author="MartinGC94"
        Description="This module serves as an alternative to the registry provider included in Powershell that gives you more flexibility in how you access registry keys."
        PowerShellVersion="5.1"
        Tags="Registry","Reg"
        ProjectUri="https://github.com/MartinGC94/PSRegistry"
        RootModule="PSRegistry.dll"
        DotNetFrameworkVersion="4.8"
        CmdletsToExport=@(
            "Add-RegKeyProperty"
            "Copy-RegKey"
            "Dismount-RegHive"
            "Get-RegKey"
            "Mount-RegHive"
            "New-RegKey"
            "Remove-RegKey"
            "Remove-RegKeyProperty"
        )
    }
}