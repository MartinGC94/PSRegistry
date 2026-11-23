---
document type: cmdlet
external help file: PSRegistry.dll-Help.xml
HelpUri: ''
Locale: da-DK
Module Name: PSRegistry
ms.date: 11-23-2026
PlatyPS schema version: 2024-05-01
title: Set-RegKeyProperty
---

# Set-RegKeyProperty

## SYNOPSIS

Creates or updates registry key properties.

## SYNTAX

### FromName (Default)

```
Set-RegKeyProperty [-Key] <RegistryKey[]> [-Name] <string[]> [[-Value] <Object>]
 [[-ValueKind] <RegistryValueType>] [-DontDisposeKey] [<CommonParameters>]
```

### FromNameRaw

```
Set-RegKeyProperty [-Key] <RegistryKey[]> [-Name] <string[]> [-RawValue] <byte[]>
 [-ValueKind] <RegistryValueType> [-DontDisposeKey] [<CommonParameters>]
```

### FromObject

```
Set-RegKeyProperty [-Key] <RegistryKey[]> -Property <RegistryProperty[]> [-DontDisposeKey]
 [<CommonParameters>]
```

### FromObjectUWP

```
Set-RegKeyProperty [-Key] <RegistryKey[]> -UwpProperty <RegistryProperty[]> [-DontDisposeKey]
 [<CommonParameters>]
```

## ALIASES

This cmdlet has the following aliases,
  Add-RegKeyProperty

## DESCRIPTION

Set-RegKeyProperty is used to add new registry properties to registry keys.  
If a key already has a property with the specified name it will be overwritten with no warning.  
This cmdlet includes some conversion logic to convert the input type into the specified valuekind.  
If no valuekind is specified it will try to guess the correct valuekind.  
The conversion details can be found under the ValueKind parameter help.

## EXAMPLES

### Example 1 Add a property and value
```powershell
PS C:\> Set-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Name InsertMode -Value 1
```

Adds the "InsertMode" property to the "HKEY_CURRENT_USER\Console" key with a value of 1.  
Because the ValueKind has not been specified and the value is an integer the property will be a Dword.

### Example 2 Add multiple properties at once with a hashtable
```powershell
PS C:\> Set-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Property @{
    InsertMode  = 1
    LineWrap    = 1
    ScrollScale = 1
    FaceName    = "Lucida Console"
}
```

Adds the 4 properties and values from the hashtable to to the "HKEY_CURRENT_USER\Console" key.  
The first 3 properties will be added as Dword because their value is an integer.  
The last property will be added as a String because its value is a string.

### Example 3 Add multiple properties with objects that specify the valuekind
```powershell
PS C:\> Set-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Property @(
    @{Name = "StringValue";       Value = "Hello"; ValueKind = "String"}
    @{Name = "MultiStringValue";  Value = "Hello"; ValueKind = "MultiString"}
    @{Name = "UwpString";         Value = "Hello"; ValueKind = "UwpString"}
    @{Name = "DwordValue";        Value = 123}
)
```
This example demonstrates how you can use an array of hashtables to force specific value kinds when the automatic detection is insufficient.  
In this example the string value will be converted to the 3 different registry string types.  
The valueKind can also be ommitted to let the command figure out the best type automatically. In this case the int value 123 is saved as a Dword value.

### Example 4 Set the default value for a registry key
```powershell
PS C:\> Add-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Name "" -Value Hello
```
This example demonstrates how to set the default value for a registry key. (The one Regedit shows as (default)).

## PARAMETERS

### -DontDisposeKey

Specifies that the registrykey should not be disposed after the properties have been added.  
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

The registry keys where the properties should be added.  
This can either be strings containing the registry key path or Registry key objects returned by Get-RegKey.  
If strings are provided the command will internally run Get-RegKey to get the registry keys with the minimum amount of permissions needed to change the property values.

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

### -Name

The name of of the properties to add.

```yaml
Type: System.String[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: FromName
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: FromNameRaw
  Position: 1
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Property

The properties to add. This includes an argumenttransformer that can handle either a single hashtable that specifies all the key/values to add.
Or an array of hashtables that define the Name, Value/RawValue, and ValueKind for each property.
See the examples section for examples of this use.

```yaml
Type: MartinGC94.PSRegistry.API.RegistryProperty[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: FromObject
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -RawValue

A raw binary representation of the object you want to add to the registry.

```yaml
Type: System.Byte[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: FromNameRaw
  Position: 2
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -UwpProperty

The properties to add. This includes an argumenttransformer that can handle either a single hashtable that specifies all the key/values to add.
Or an array of hashtables that define the Name, Value/RawValue, and ValueKind for each property.
See the examples section for examples of this use.
This is identical to the Property parameter, except it will try to convert values to UWP values, instead of classic registry values.

```yaml
Type: MartinGC94.PSRegistry.API.RegistryProperty[]
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: FromObjectUWP
  Position: Named
  IsRequired: true
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -Value

The value to add.

```yaml
Type: System.Object
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: FromName
  Position: 2
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
DontShow: false
AcceptedValues: []
HelpMessage: ''
```

### -ValueKind

The registry type (String, Dword, etc.) of the value that will be added.
If this parameter is specified, the command will try to convert the inputdata using the standard PowerShell conversion rules.
(Internally the command simply calls LanguagePrimitives.ConvertTo<T> where T is the .NET type that most closely matches the Registry type).
In the case of strings and multistrings the command will also append the necessary terminating null chars before converting the value to binary.
In the case of UWP values a timestamp for the current time is appended to the binary value.

If this parameter is not specified, the cmdlet will try to find the best type based on the type of the Value parameter.
string = String.
string[] = MultiString
byte[] = Binary
int = Dword
uint = Dword
ulong = Qword
long = Qword
bool = Dword
SwitchParameter = Dword
Object[] if the first Value is a byte, and the whole array can be converted to a byte array then Binary.
Object[] if the first value is a string, and the whole array can be converted to a string array then MultiString.

```yaml
Type: MartinGC94.PSRegistry.API.RegistryValueType
DefaultValue: ''
SupportsWildcards: false
Aliases: []
ParameterSets:
- Name: FromName
  Position: 3
  IsRequired: false
  ValueFromPipeline: false
  ValueFromPipelineByPropertyName: false
  ValueFromRemainingArguments: false
- Name: FromNameRaw
  Position: 3
  IsRequired: true
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

