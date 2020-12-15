---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version: 1.0.0
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
If a key already has a property with the specified name it will be overwritten.

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
The first 3 properties will be added as Dword because the value is an integer.  
The last property will be added as a String because the value is a string.

### Example 3 Add multiple properties with objects that specify the valuekind
```powershell
PS C:\> Add-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Property @(
    [PSRegistry.RegistryProperty]@{
        Name      = "SomeBinaryValue"
        Value     = @(0,0,1,1,0)
        ValueKind = [Microsoft.Win32.RegistryValueKind]::Binary
    }
    [PSRegistry.RegistryProperty]@{
        Name      = "SomeMultiStringValue"
        Value     = @("String1","String2")
        ValueKind = [Microsoft.Win32.RegistryValueKind]::MultiString
    }
)
```

Adds the 2 properties and values from the array to the "HKEY_CURRENT_USER\Console" key.  
This can be used when multiple properties with different types have to be added to a key.

## PARAMETERS

### -Key
The registry keys where the properties should be added.  
This can either be a string with the registry key path or it can be a Registry key object returned by Get-RegKey.

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
Everything else = The result from the ToString method.

Binary:  
Null = Empty byte array  
String = The unicode representation of the string is converted to bytes.  
Generic array = Each element is cast to int and then to byte.  
Int = The int is cast to a byte and saved in a byte array.

Dword or Qword:  
Null = 0

MultiString:  
Null = Empty string array.  
Generic array = The ToString method is run on every element.  
Everything else except string arrays = The ToString method is run on the object and put into a single element string array.

Unknown or None:  
Null = Empty String  
Generic array where first element is an interger = Binary. (Every element is cast to int, then byte).  
Generic array where first value is a string = MultiString value. (ToString is run on each element).

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
The properties to add. This can either be a hashtable with the property names and values, or a [PSRegistry.RegistryProperty] object.

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
