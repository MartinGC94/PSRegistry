---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version:
schema: 2.0.0
---

# Add-RegKeyProperty

## SYNOPSIS
Creates or updates registry key properties.

## SYNTAX

### FromName (Default)
```
Add-RegKeyProperty [-Key] <RegistryKey[]> [-Name] <String[]> [[-Value] <Object>] [[-ValueKind] <RegistryValueKind>] [-DontDisposeKey] [<CommonParameters>]
```

### FromObject
```
Add-RegKeyProperty [-Key] <RegistryKey[]> -Property <RegistryProperty[]> [-DontDisposeKey] [<CommonParameters>]
```

## DESCRIPTION
Add-RegKeyProperty is used to add new registry properties to registry keys.  
If a key already has a property with the specified name it will be overwritten with no warning.  
This cmdlet includes some conversion logic to convert the input type into the specified valuekind.  
If no valuekind is specified, or it's set to "Unknown" it will try to guess the correct valuekind.  
The conversion details can be found under the ValueKind parameter help.

## EXAMPLES

### Example 1 Add a property and value
```powershell
PS C:\> Add-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Name InsertMode -Value 1
```

Adds the "InsertMode" property to the "HKEY_CURRENT_USER\Console" key with a value of 1.  
Because the ValueKind has not been specified and the value is an integer the property will be a Dword.

### Example 2 Add multiple properties at once with a hashtable
```powershell
PS C:\> Add-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Property @{
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
PS C:\> Add-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Property @(
    [PSRegistry.RegistryProperty]@{Name = "BinaryValue";       Value = "Hello"; ValueKind = [Microsoft.Win32.RegistryValueKind]::Binary}
    [PSRegistry.RegistryProperty]@{Name = "MultiStringValue";  Value = "Hello"; ValueKind = [Microsoft.Win32.RegistryValueKind]::MultiString}
    [PSRegistry.RegistryProperty]@{Name = "StringValue";       Value = "Hello"; ValueKind = [Microsoft.Win32.RegistryValueKind]::String}
    [PSRegistry.RegistryProperty]@{Name = "DefaultValue";      Value = "Hello"; ValueKind = [Microsoft.Win32.RegistryValueKind]::Unknown}
    [PSRegistry.RegistryProperty]@{Name = "DefaultValue2";     Value = "Hello"}
    [PSRegistry.RegistryProperty]@{Name = "DefaultValue3"}
)
```
This example demonstrates how you can use the RegistryProperty object type to force specific value kinds when the automatic detection is insufficient.  
In this example the string value will be converted to different types to match the valuekind, the conversion rules can be read under the ValueKind parameter.  
Leaving out the ValueKind is the same as setting it to "Unknown". Leaving out the value and ValueKind will create an empty string property.

### Example 4 Set the default value for a registry key
```powershell
PS C:\> Add-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Name "" -Value Hello
```
This example demonstrates how to set the default value for a registry key. (The one Regedit shows as (default)).

## PARAMETERS

### -Key
The registry keys where the properties should be added.  
This can either be strings containing the registry key path or Registry key objects returned by Get-RegKey.  
If strings are provided the command will internally run Get-RegKey to get the registry keys with the minimum amount of permissions needed to change the property values.

```yaml
Type: RegistryKey[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Name
The name of of the properties to add.

```yaml
Type: String[]
Parameter Sets: FromName
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
The value to add.

```yaml
Type: Object
Parameter Sets: FromName
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ValueKind
The registry type (String, Dword, etc.) of the value that will be added. By default it is set to Unknown where the cmdlet will find an appropriate type.

String and ExpandStrings are handled like this:  
Null = Empty string  
Everything else = The result from the ToString method on the input object.

Binary and None:  
Null = Empty byte array  
String = The unicode representation of the string is converted to bytes.  
Int = The int is converted to a byte and saved in a single element byte array.  
IList (lists and arrays) = Each element is converted to bytes.

Dword or Qword:  
Null = 0

MultiString:  
Null = Empty string array.  
String = Single element string array.
IList (lists and arrays) = The ToString method is run on every element.  
Everything else except string arrays = The ToString method is run on the object and put into a single element string array.

Unknown:  
Null = Empty String  
IList (lists and arrays) where first element is a byte = Binary. (Every element is converted to byte).  
IList (lists and arrays) where first element is a string = MultiString value. (ToString is run on each element).

Every other scenario is handled automatically by the underlying [SetValue](https://docs.microsoft.com/en-us/dotnet/api/microsoft.win32.registrykey.setvalue) method.

```yaml
Type: RegistryValueKind
Parameter Sets: FromName
Aliases:
Accepted values: Unknown, String, ExpandString, Binary, DWord, MultiString, QWord, None

Required: False
Position: 3
Default value: Unknown
Accept pipeline input: False
Accept wildcard characters: False
```

### -Property
The properties to add. This can either be a hashtable with the property names and values, or an array of [PSRegistry.RegistryProperty] objects.

```yaml
Type: RegistryProperty[]
Parameter Sets: FromObject
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```



### -DontDisposeKey
Specifies that the registrykey should not be disposed after the properties have been added.  
This is useful when you need to do multiple operations on a registry key and plan on manually disposing it when done.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: DontCloseKey, NoDispose

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```




### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Microsoft.Win32.RegistryKey[]

## OUTPUTS

### None
## NOTES

## RELATED LINKS
