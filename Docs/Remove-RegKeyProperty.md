---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version: 1.0.0
schema: 2.0.0
---

# Remove-RegKeyProperty

## SYNOPSIS
Deletes registry properties

## SYNTAX

```
Remove-RegKeyProperty [-Key] <RegistryKey[]> [-PropertyName] <String[]> [-DontDisposeKey] [-WhatIf] [-Confirm] [<CommonParameters>]
```

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

### -Key
The registry keys where the properties should be removed.  
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

### -PropertyName
The properties to remove.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: Name

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DontDisposeKey
Specifies that the registrykey should not be disposed after the properties have been deleted.  
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



### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```


### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

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
