#Requires -Version 5.1 -Modules platyPS

[CmdletBinding(SupportsShouldProcess, ConfirmImpact = 'High')]
param
(
    [Parameter(Mandatory)]
    [Alias("ModuleVersion")]
    [version]$Version,

    [Parameter()]
    [string]$Destination = "$PSScriptRoot\Releases",

    [Parameter()]
    [Validateset("Release","Debug")]
    [string]$Configuration = "Release"

)
$ModuleName="PSRegistry"

#region Create destination folder and make sure it is empty
$DestinationDirectory = [System.IO.Path]::Combine($Destination, $ModuleName, $Version)
$null = New-Item -Path $DestinationDirectory -ItemType Directory -Force

$ItemsToRemove = Get-ChildItem -LiteralPath $DestinationDirectory -Force
if ($ItemsToRemove -and $PSCmdlet.ShouldProcess($DestinationDirectory, "Deleting $($ItemsToRemove.Count) item(s)"))
{
    Remove-Item -LiteralPath $ItemsToRemove.FullName -Recurse -Force
}
#endregion

#region Compile and add all content to destination folder
MSBuild.exe "$PSScriptRoot\$ModuleName.sln" "-property:Configuration=$Configuration,Version=$Version"

$CompiledFiles = Get-ChildItem -Path "$PSScriptRoot\src\PSRegistry\bin\*\$Configuration" -Recurse -Filter "$ModuleName.dll"

$FileToImport = $CompiledFiles | Where-Object -Property FullName -Like "$PSScriptRoot\*\$PSEdition\*" | Select-Object -First 1
$ModuleInfo = Import-Module -Name $FileToImport.FullName -PassThru
$CmdletsToExport = $ModuleInfo.ExportedCmdlets.Keys.ForEach({"'$_'"}) -join ','
Remove-Module -ModuleInfo $ModuleInfo

Copy-Item -LiteralPath $CompiledFiles.DirectoryName -Destination $DestinationDirectory -Recurse -Container -Force -Filter "$ModuleName.dll"
Copy-Item -Path "$PSScriptRoot\PowershellFormats\*" -Destination $DestinationDirectory -Force
Copy-Item -LiteralPath "$PSScriptRoot\ModuleManifest.psd1" -Destination "$DestinationDirectory\$ModuleName.psd1"

$HelpFile = New-ExternalHelp -Path $PSScriptRoot\Docs -OutputPath $DestinationDirectory\en-US
[xml]$HelpContent = Get-Content -LiteralPath $HelpFile.FullName -Raw
$HelpExamples = Select-Xml -Xml $HelpContent.helpItems -XPath //command:example -Namespace @{command = "http://schemas.microsoft.com/maml/dev/command/2004/10"}
foreach ($Example in $HelpExamples)
{
    for ($i = 0; $i -lt 2; $i++)
    {
        $Element = $HelpContent.CreateElement('maml', 'para', 'http://schemas.microsoft.com/maml/2004/10')
        $null = $Example.Node.remarks.AppendChild($Element)
    }
}
$HelpContent.Save($HelpFile.FullName)
#endregion

#region update module manifest
$FileList = (Get-ChildItem -LiteralPath $DestinationDirectory -File -Recurse | ForEach-Object -Process {
    "'$($_.FullName.Replace("$DestinationDirectory\", ''))'"
}) -join ','
$ReleaseNotes = Get-Content -LiteralPath "$PSScriptRoot\Release notes.txt" -Raw

((Get-Content -LiteralPath "$PSScriptRoot\ModuleManifest.psd1" -Raw) -replace '{(?=[^\d])','{{' -replace '(?<!\d)}','}}') -f @(
    "'$Version'"
    $CmdletsToExport
    $FileList
    $ReleaseNotes
) | Set-Content -LiteralPath "$DestinationDirectory\$ModuleName.psd1" -Force
#endregion