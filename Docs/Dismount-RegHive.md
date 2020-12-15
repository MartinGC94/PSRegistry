---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version: 1.0.0
schema: 2.0.0
---

# Dismount-RegHive

## SYNOPSIS
Dismounts a mounted registry hive.

## SYNTAX

```
Dismount-RegHive [-Path] <String[]> [[-ComputerName] <String[]>] [-View <RegistryView>] [<CommonParameters>]
```

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

### -Path
The path of the registry hive that should be dismounted.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ComputerName
The computer where the registry hive should be dismounted from.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### None
## NOTES

## RELATED LINKS
