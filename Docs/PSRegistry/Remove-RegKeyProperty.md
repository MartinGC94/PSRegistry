---
document type: cmdlet
external help file: PSRegistry.dll-Help.xml
HelpUri: ''
Locale: da-DK
Module Name: PSRegistry
ms.date: 11-23-2026
PlatyPS schema version: 2024-05-01
title: Remove-RegKeyProperty
---

# Remove-RegKeyProperty

## SYNOPSIS

Deletes registry properties

## SYNTAX

### __AllParameterSets

```
Remove-RegKeyProperty [-Key] <RegistryKey[]> [-PropertyName] <string[]> [-DontDisposeKey] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  None

## DESCRIPTION

The Remove-RegKeyProperty cmdlet deletes registry key properties.

## EXAMPLES

### Example 1 Remove a property from a key.
```powershell
PS C:\> Remove-RegKeyProperty -Key HKEY_CURRENT_USER\SomeKey -PropertyName "SomeProperty"
```

Removes the "SomeProperty" property from "HKEY_CURRENT_USER\SomeKey".

### Example 2 Remove the default property for a key.
```powershell
PS C:\> Remove-RegKeyProperty -Key HKEY_CURRENT_USER\SomeKey -PropertyName ""
```

Removes the default property from "HKEY_CURRENT_USER\SomeKey".

## PARAMETERS

### -Confirm

Prompts you for confirmation before running the cmdlet.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases:
- cf
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

### -DontDisposeKey

Specifies that the registrykey should not be disposed after the properties have been deleted.  
This is useful when you need to do multiple operations on a registry key and plan on manually disposing it when done.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases:
- DontCloseKey
- NoDispose
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

### -Key

The registry keys where the properties should be removed.  
This can either be a string with the registry key path or it can be a Registry key object returned by Get-RegKey.

```yaml
Type: Microsoft.Win32.RegistryKey[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: (All)
  Position: 0
  IsRequired: true
  ValueFromPipeline: true
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -PropertyName

The properties to remove.

```yaml
Type: System.String[]
DefaultValue: ''
SupportsWildcards: false
Aliases:
- Name
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

### -WhatIf

Runs the command in a mode that only reports what would happen without performing the actions.

```yaml
Type: System.Management.Automation.SwitchParameter
DefaultValue: ''
SupportsWildcards: false
Aliases:
- wi
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

### Microsoft.Win32.RegistryKey

{{ Fill in the Description }}

### Microsoft.Win32.RegistryKey[]

{{ Fill in the Description }}

## OUTPUTS

### System.Object

{{ Fill in the Description }}

## NOTES

{{ Fill in the Notes }}

## RELATED LINKS

{{ Fill in the related links here }}

