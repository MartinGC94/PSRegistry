---
document type: cmdlet
external help file: PSRegistry.dll-Help.xml
HelpUri: ''
Locale: da-DK
Module Name: PSRegistry
ms.date: 11-23-2026
PlatyPS schema version: 2024-05-01
title: New-RegKey
---

# New-RegKey

## SYNOPSIS

Creates new registry keys.

## SYNTAX

### __AllParameterSets

```
New-RegKey [-Path] <string[]> [[-ComputerName] <string[]>]
 [-KeyPermissionCheck <RegistryKeyPermissionCheck>] [-View <RegistryView>]
 [-Options <RegistryOptions>] [-ACL <RegistrySecurity>] [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  None

## DESCRIPTION

The New-RegKey cmdlet creates new registry keys.  
If a key already exists the key will be written to the pipeline the same way it would with non-existing keys.  
A custom ACL can be set for newly created keys but it will not change the ACL for keys that already exist.  
Keys can be created as volatile, meaning that they will be deleted when the computer is shutdown or restarted.

## EXAMPLES

### Example 1 Create a new registry key
```powershell
PS C:\> New-RegKey -Path HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware
```

Create the new registry key "HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware".

### Example 2 Create a new volatile registry key
```powershell
PS C:\> New-RegKey -Path HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware\TempKey -Options Volatile
```

Create a volatile key "HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware\TempKey" that will be deleted when the registry hive is unloaded (when the computer is shutdown/rebooted).

## PARAMETERS

### -ACL

Specifies a custom ACL to set on the newly created key.  
Note: If the key already exists the ACL will not change.

```yaml
Type: System.Security.AccessControl.RegistrySecurity
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

### -ComputerName

The computer where the registry key should be created.

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

### -Options

Allows you to specify that the key is "Volatile" meaning that it will be deleted when the registry hive is unloaded.

```yaml
Type: Microsoft.Win32.RegistryOptions
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

The full path to the key that should be created.

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

