---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version:
schema: 2.0.0
---

# Rename-RegKey

## SYNOPSIS
Renames registry keys.

## SYNTAX

```
Rename-RegKey [-Path] <string[]> [-NewName] <string> [-View <RegistryView>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Rename-RegKey cmdlet renames registry keys using the [RegRenameKey](https://docs.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regrenamekey) function from winreg.h.  
If a key with the NewName already exists the command will fail.

## EXAMPLES

### Example 1 Rename a single key
```powershell
PS C:\> Rename-RegKey -Path 'HKEY_CURRENT_USER\Console\Demokey1' -NewName 'DemoKey2'
```

## PARAMETERS

### -Path
Specifies the full path to the registry key that should be renamed.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: Name

Required: True
Position: 0
Default value: None
Accept pipeline input: True
Accept wildcard characters: False
```

### -NewName
Specifies the new name for the registry key.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
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
