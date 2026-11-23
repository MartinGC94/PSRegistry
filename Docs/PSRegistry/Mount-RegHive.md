---
document type: cmdlet
external help file: PSRegistry.dll-Help.xml
HelpUri: ''
Locale: da-DK
Module Name: PSRegistry
ms.date: 11-23-2026
PlatyPS schema version: 2024-05-01
title: Mount-RegHive
---

# Mount-RegHive

## SYNOPSIS

Mounts registry hive files as keys in the registry.

## SYNTAX

### __AllParameterSets

```
Mount-RegHive [-Path] <string> [-DestinationPath] <string> [[-ComputerName] <string>]
 [-View <RegistryView>] [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  None

## DESCRIPTION

The Mount-RegHive cmdlet mounts registry database files as keys using the [RegLoadKey](https://docs.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regloadkeya) function in winreg.h.  
This will allow you to make changes to the registry for offline Windows images, or users that aren't logged in.

## EXAMPLES

### Example 1 Mount the default user registry file
```powershell
PS C:\> Mount-RegHive -Path "C:\Users\Default\NTUSER.DAT" -DestinationPath HKLM:\TempMount
```

Mounts the default user registry hive to the HKLM:\TempMount key.

## PARAMETERS

### -ComputerName

The computer where the registry hive should be mounted.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 2
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -DestinationPath

The registry key path where the registry hive should be mounted.  
This can only be a direct subkey from a basekey like "HKLM:\TempMount".

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases:
- MountPath
ParameterSets:
- Name: (All)
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Path

The path to the file containing the registry database that should be mounted.  
If the command is targeting a remote computer then the path will be relative to that remote computer.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases:
- FilePath
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

### System.Object

{{ Fill in the Description }}

## NOTES

{{ Fill in the Notes }}

## RELATED LINKS

{{ Fill in the related links here }}

