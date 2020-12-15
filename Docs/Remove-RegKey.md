---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version: 1.0.0
schema: 2.0.0
---

# Remove-RegKey

## SYNOPSIS
Deletes registry keys.

## SYNTAX

```
Remove-RegKey [-Path] <String[]> [[-ComputerName] <String[]>] [-Recurse] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-RegKey cmdlet deletes registry keys.  
If the Recurse parameter is not specified the cmdlet will not delete keys that contain subkeys.

## EXAMPLES

### Example 1 Delete a registry key along with any subkeys it may have
```powershell
PS C:\> Remove-RegKey -Path HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware -Recurse
```

Deletes the HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware registry key along with any subkeys.

## PARAMETERS

### -Path
The registry key that should be deleted.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: Name

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ComputerName
The computer where the registry key should be deleted.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Recurse
Allows the cmdlet to delete registry keys that contain subkeys.  
If this is not set and a specified key contains subkeys the cmdlet will write an error and skip that key.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -View
Specifies the registry view to target.  
This allows you view the registry like a 32-bit application would on a 64-bit OS.

```yaml
Type: RegistryView
Parameter Sets: (All)
Aliases:
Accepted values: Default, Registry64, Registry32

Required: False
Position: Named
Default value: Default
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

### System.String[]

## OUTPUTS

### None
## NOTES

## RELATED LINKS
