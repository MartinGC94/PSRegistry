#Requires -Version 5.1 -Modules Microsoft.PowerShell.PlatyPS
param
(
    [Parameter(Mandatory)]
    [string]
    $OutputFile
)

# Lazy hack to offset the two project builds at the same time so there's less chance for write conflicts.
# Ideally I'd update the script and MSBuild logic to only run the script once for the entire solution.
Start-Sleep -Milliseconds (100 * (Get-Random -Minimum 0 -Maximum 30))

$ModuleName = [System.IO.Path]::GetFileNameWithoutExtension($OutputFile)
$OutputDir = Split-Path -LiteralPath $OutputFile

$ReleaseNotes = Get-Content -LiteralPath "$PSScriptRoot\Release notes.txt" -Raw
$Version = $ReleaseNotes.Split(':')[0]
$ReleaseDir = Join-Path -Path $PSScriptRoot -ChildPath "Releases\$ModuleName\$Version"
$null = New-Item -Path $ReleaseDir -ItemType Directory -Force
Copy-Item -LiteralPath $OutputDir -Destination $ReleaseDir -Container -Recurse -Force -ErrorAction Stop

$FormatFile = Copy-Item -LiteralPath "$PSScriptRoot\${ModuleName}Format.ps1xml" -Destination $ReleaseDir -Force -PassThru

$ModuleInfo = Import-Module -Name $OutputFile -PassThru -ErrorAction Stop
$CmdletsToExport = $ModuleInfo.ExportedCmdlets.Keys.ForEach({"'$_'"}) -join ','
$AliasesToExport = $ModuleInfo.ExportedAliases.Keys.ForEach({"'$_'"}) -join ','
Remove-Module -ModuleInfo $ModuleInfo

if (Test-Path -LiteralPath "$PSScriptRoot\Docs")
{
    $null = New-Item -Path "$ReleaseDir\en-US" -ItemType Directory -Force
    Get-ChildItem -LiteralPath "$PSScriptRoot\Docs" -Recurse -File -Filter *.md |
        Import-MarkdownCommandHelp |
        Export-MamlCommandHelp -OutputFolder $env:TEMP -Force |
        Move-Item -Destination "$ReleaseDir\en-US" -Force
}

#region Update module manifest
$null = New-Item -Path "$ReleaseDir\$ModuleName.psd1" -ItemType File -Force

$FileList = (Get-ChildItem -LiteralPath $ReleaseDir -File -Recurse | ForEach-Object -Process {
    "'$($_.FullName.Replace("$ReleaseDir\", ''))'"
}) -join ','



((Get-Content -LiteralPath "$PSScriptRoot\ModuleManifest.psd1" -Raw) -replace '{(?=[^\d])','{{' -replace '(?<!\d)}','}}') -f @(
    "'$Version'"
    "'$($FormatFile.FullName.Replace("$ReleaseDir\", ''))'"
    $CmdletsToExport
    $AliasesToExport
    $FileList
    $ReleaseNotes
) | Set-Content -LiteralPath "$ReleaseDir\$ModuleName.psd1" -Force
#endregion