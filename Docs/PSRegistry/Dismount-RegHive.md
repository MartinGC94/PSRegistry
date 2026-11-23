---
document type: cmdlet
external help file: PSRegistry.dll-Help.xml
HelpUri: ''
Locale: da-DK
Module Name: PSRegistry
ms.date: 11-23-2026
PlatyPS schema version: 2024-05-01
title: Dismount-RegHive
---

# Dismount-RegHive

## SYNOPSIS

Dismounts a mounted registry hive.

## SYNTAX

### __AllParameterSets

```
Dismount-RegHive [-Path] <string> [[-ComputerName] <string>] [-View <RegistryView>]
 [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  None

## DESCRIPTION

This cmdlet dismounts registry hives using the [RegUnLoadKey](https://docs.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regunloadkeya) function in winreg.h.

## EXAMPLES

### Example 1 Dismount a registry hive
```powershell
PS C:\> Dismount-RegHive -Path HKLM:\TempMount
```

Dismounts the registry hive that was mounted in HKLM:\TempMount on the local computer.

### Example 2 Dismount a registry hive on a remote computer
```powershell
PS C:\> Dismount-RegHive -Path HKLM:\TempMount -ComputerName Server1
```

Dismounts the registry hive that was mounted in HKLM:\TempMount on the "Server1" computer.

## PARAMETERS

### -ComputerName

The computer where the registry hive should be dismounted from.

```yaml
Type: System.String
DefaultValue: ''
SupportsWildcards: false
Aliases: []
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

### -Path

The path of the registry hive that should be dismounted.

```yaml
Type: System.String
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

### System.Object

{{ Fill in the Description }}

## NOTES

{{ Fill in the Notes }}

## RELATED LINKS

{{ Fill in the related links here }}

