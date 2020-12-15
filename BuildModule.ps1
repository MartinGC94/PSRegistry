#Requires -Version 5.1

[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'High')]
param
(
    [Parameter(Mandatory)]
    [Alias("ModuleVersion")]
    [version]$Version,

    [string]$Destination = "$PSScriptRoot\Releases",

    [Parameter()]
    [Validateset("Release","Debug")]
    [string]$Configuration="Release"

)

#region import module data
$ModuleData = Import-PowerShellDataFile -Path "$PSScriptRoot\ModuleData.psd1"

$ModuleName     = $ModuleData["ModuleName"]
$ModuleManifest = $ModuleData["ManifestData"]
$RootModuleName = $ModuleManifest["RootModule"]

$ModuleManifest.Add("ReleaseNotes", (Get-Content -Path "$PSScriptRoot\Release Notes.txt" -Raw -Encoding Default))
$ModuleManifest.Add("ModuleVersion", $Version)
#endregion


#region Create destination folder and make sure it is empty
$DestinationDirectory = [System.IO.Path]::Combine($Destination, $ModuleName, $Version)
[void](New-Item -Path $DestinationDirectory -ItemType Directory -Force)

$ItemsToRemove = Get-ChildItem -Path $DestinationDirectory
if ($ItemsToRemove -and $PSCmdlet.ShouldProcess($DestinationDirectory, "Deleting $($ItemsToRemove.Count) item(s)"))
{
    Remove-Item -LiteralPath $ItemsToRemove.FullName -Recurse -Force
}
#endregion

#region update assembly version info
$AssemblyInfoFile=Get-ChildItem -Path $PSScriptRoot\$ModuleName -Filter AssemblyInfo.cs -Recurse -File | Select-Object -First 1
[string[]]$Content=Get-Content -LiteralPath $AssemblyInfoFile.FullName -Encoding UTF8

$Line=Select-String -LiteralPath $AssemblyInfoFile.FullName -Pattern '^\[assembly: AssemblyVersion\(\"[0-9]\.'
$Content[$Line.LineNumber-1]="[assembly: AssemblyVersion(`"$($Version.ToString())`")]"
$Line=Select-String -LiteralPath $AssemblyInfoFile.FullName -Pattern '^\[assembly: AssemblyFileVersion\(\"[0-9]\.'
$Content[$Line.LineNumber-1]="[assembly: AssemblyFileVersion(`"$($Version.ToString())`")]"

Set-Content -Value $Content -LiteralPath $AssemblyInfoFile.FullName -Encoding UTF8
#endregion


MSBuild.exe $PSScriptRoot\$ModuleName.sln /property:Configuration=$Configuration


#region Add all module content to $DestinationDirectory
Copy-Item -Path "$PSScriptRoot\$ModuleName\bin\$Configuration\$ModuleName.dll" -Destination $DestinationDirectory

$FormatFiles=Get-ChildItem -Path "$PSScriptRoot\PowershellFormats" -File
Copy-Item -Path $FormatFiles.FullName -Destination $DestinationDirectory

New-ModuleManifest @ModuleManifest -Path "$DestinationDirectory\$ModuleName.psd1" -FormatsToProcess $FormatFiles.Name
New-ExternalHelp -Path $PSScriptRoot\Docs -OutputPath $DestinationDirectory

$FilesInModule = Get-ChildItem -Path $DestinationDirectory -Recurse -File -Force
Update-ModuleManifest -Path "$DestinationDirectory\$ModuleName.psd1" -FileList $FilesInModule.FullName.Replace("$DestinationDirectory\", '')
#endregion