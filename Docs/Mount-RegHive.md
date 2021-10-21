---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version:
schema: 2.0.0
---

# Mount-RegHive

## SYNOPSIS
Mounts registry hive files as keys in the registry.

## SYNTAX

```
Mount-RegHive [-Path] <String> [-DestinationPath] <String> [[-ComputerName] <String>] [-View <RegistryView>] [<CommonParameters>]
```

## DESCRIPTION
The Mount-RegHive cmdlet mounts registry database files as keys using the [RegLoadKey](https://docs.microsoft.com/en-us/windows/win32/api/winreg/nf-winreg-regloadkeya) function in winreg.h.  
This will allow you to make changes to the registry for offline Windows images, or users that aren't logged in.

## EXAMPLES

### Example 1 Mount the default user registry file
```powershell
PS C:\> Mount-RegHive -Path "C:\Users\Default\NTUSER.DAT" -DestinationPath HKLM:\TempMount
```

Mounts the default user registry hive to the HKLM:\TempMount key.


## PARAMETERS

### -Path
The path to the file containing the registry database that should be mounted.  
If the command is targeting a remote computer then the path will be relative to that remote computer.

```yaml
Type: String
Parameter Sets: (All)
Aliases: FilePath

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DestinationPath
The registry key path where the registry hive should be mounted.  
This can only be a direct subkey from a basekey like "HKLM:\TempMount".

```yaml
Type: String
Parameter Sets: (All)
Aliases: MountPath

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```


### -ComputerName
The computer where the registry hive should be mounted.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### None
## NOTES

## RELATED LINKS
