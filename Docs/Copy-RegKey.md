---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version:
schema: 2.0.0
---

# Copy-RegKey

## SYNOPSIS
Copies a registry key (with subkeys and values) to another key.

## SYNTAX

```
Copy-RegKey [-Key] <RegistryKey> [-Destination] <String[]> [[-ComputerName] <String[]>] [-View <RegistryView>] [-DontDisposeKey] [-DestinationKeyRights <RegistryRights>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet copies a source key to one or more destinations using the [RegCopyTree](https://docs.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regcopytreea) function from winreg.h.  
The source will be merged into the destination, replacing any existing properties with the source value and type.

## EXAMPLES

### Example 1 Simple copy
```powershell
PS C:\> Copy-RegKey -Key HKCU:\Console\ -Destination HKCU:\Console2\
```

Copies the "HKCU:\Console" key to "HKCU:\Console2".

### Example 2 Copying a local key to a remote computer
```powershell
PS C:\> Copy-RegKey -Key HKCU:\Console\ -Destination HKEY_USERS\.DEFAULT\Console -ComputerName Computer1
```

Copies the "HKCU:\Console" key from the local machine to "HKEY_USERS\.DEFAULT\Console" on the remote "Computer1" machine.

## PARAMETERS

### -Key
The registry key that should be copied.  
This can either be a string with the registry key path or it can be a Registry key object returned by Get-RegKey.  
If a string is provided the command will internally run Get-RegKey to get the registry key with the minimum amount of permissions needed to copy a key + values and subkeys.

```yaml
Type: RegistryKey
Parameter Sets: (All)
Aliases: Source, Path

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Destination
The registry path where the key should be copied to.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ComputerName
The remote computer for the "Destination" parameter.  
To specify the computername for the "Key" parameter use Get-RegKey with the ComputerName parameter.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: PSComputerName

Required: False
Position: 2
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

### -DontDisposeKey
Specifies that the source registry key should not be disposed after copying to the destinations.  
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

### -DestinationKeyRights
Specifies the RegistryRights used to open the destination key.  
If this is not set the command will use the same RegistryRights used to open the source key.

```yaml
Type: RegistryRights
Parameter Sets: (All)
Aliases:

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

### Microsoft.Win32.RegistryKey

## OUTPUTS

### None
## NOTES

## RELATED LINKS
