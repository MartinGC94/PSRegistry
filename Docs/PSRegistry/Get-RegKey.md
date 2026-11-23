---
document type: cmdlet
external help file: PSRegistry.dll-Help.xml
HelpUri: ''
Locale: da-DK
Module Name: PSRegistry
ms.date: 11-23-2026
PlatyPS schema version: 2024-05-01
title: Get-RegKey
---

# Get-RegKey

## SYNOPSIS

Gets registry key(s) along with their properties.

## SYNTAX

### __AllParameterSets

```
Get-RegKey [-Path] <string[]> [[-ComputerName] <string[]>] [-Recurse] [-Depth <int>]
 [-KeyPermissionCheck <RegistryKeyPermissionCheck>] [-Rights <RegistryRights>]
 [-View <RegistryView>] [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  None

## DESCRIPTION

The Get-RegKey cmdlet retrieves registry keys and properties.  
The command has several options for how it should read the registry keys that affects performance and what you can do with the registry key objects returned by the cmdlet.  
The default values for the various options are optimized for making changes to registry keys.

## EXAMPLES

### Example 1 Get a registry key and save it in a variable
```powershell
PS C:\> $ConsoleKey=Get-RegKey -Path HKCU:\Console
```

Returns the "HKCU:\Console" key with write permissions.

### Example 2 Get a key and its subkeys up to a specific depth
```powershell
PS C:\> $ConsoleKeys=Get-RegKey -Path HKCU:\Console -Recurse -Depth 2
```

Returns the subkeys under "HKCU:\Console" up to 2 levels down from the "HKCU:\Console" key.  

### Example 3 Get a registry key with only the permissiones required to read the key and properties
```powershell
PS C:\> $ConsoleKeys=Get-RegKey -Path HKCU:\Console -Recurse -KeyPermissionCheck ReadSubTree -Rights EnumerateSubKeys,QueryValues
```

Returns the "HKCU:\Console" key and subkeys with the minimum required permissions required for reading.

### Example 4 Take ownership of a key and grant yourself full control permissions
```powershell
PS C:\> $RegKey = Get-RegKey -Path 'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\AlternateShells' -KeyPermissionCheck ReadWriteSubTree -Rights TakeOwnership
$ACL = $RegKey.GetAccessControl()
$ACL.SetOwner([System.Security.Principal.NTAccount]::new("$env:USERNAME"))
$RegKey.SetAccessControl($ACL)
$RegKey.Dispose()
$RegKey = Get-RegKey -Path 'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\AlternateShells' -KeyPermissionCheck ReadWriteSubTree -Rights ChangePermissions
$ACL.AddAccessRule([System.Security.AccessControl.RegistryAccessRule]::new($env:USERNAME,"FullControl",("ContainerInherit","ObjectInherit"),"none","Allow"))
$RegKey.SetAccessControl($ACL)
$RegKey.Dispose()
```

Takes ownership of a key and grants the user full control.  
Note that the powershell process needs to have the SeTakeOwnershipPrivilege enabled for this to work.

## PARAMETERS

### -ComputerName

The computername to get the key from.

```yaml
Type: System.String[]
DefaultValue: ''
SupportsWildcards: false
Aliases:
- PSComputerName
ParameterSets:
- Name: (All)
  Position: 1
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Depth

Specifies how deep the command should look for keys when the Recurse parameter is specified.  
The depth is relative to each input path.

```yaml
Type: System.Int32
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -KeyPermissionCheck

Specifies whether security checks are performed when opening registry keys and accessing their name/value pairs.  
Default: The registry key inherits the mode of its parent. Security checks are performed when trying to access subkeys or values, unless the parent was opened with ReadSubTree or ReadWriteSubTree mode.  
ReadSubTree: Security checks are not performed when accessing subkeys or values. A security check is performed when trying to open the current key, unless the parent was opened with ReadSubTree or ReadWriteSubTree.  
ReadWriteSubTree: Security checks are not performed when accessing subkeys or values. A security check is performed when trying to open the current key, unless the parent was opened with ReadWriteSubTree.

```yaml
Type: Microsoft.Win32.RegistryKeyPermissionCheck
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Path

Specifies the path to get the registry keys from.

```yaml
Type: System.String[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 0
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Recurse

Specifies if the cmdlet should get subkeys from each path specified.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Rights

Specifies the rights used when accessing each key.  
The default rights allow you to read/write registry keys.

```yaml
Type: System.Security.AccessControl.RegistryRights
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -View

Specifies the registry view to target.  
This allows you view the registry like a 32-bit application would on a 64-bit OS.

```yaml
Type: Microsoft.Win32.RegistryView
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: Named
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutBuffer, -OutVariable, -PipelineVariable,
-ProgressAction, -Verbose, -WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](https://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

## OUTPUTS

### Microsoft.Win32.RegistryKey

{{ Fill in the Description }}

## NOTES

{{ Fill in the Notes }}

## RELATED LINKS

{{ Fill in the related links here }}

