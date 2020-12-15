---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version: 1.0.0
schema: 2.0.0
---

# New-RegKey

## SYNOPSIS
Creates new registry keys.

## SYNTAX

```
New-RegKey [-Path] <String[]> [[-ComputerName] <String[]>] [-KeyPermissionCheck <RegistryKeyPermissionCheck>]
 [-View <RegistryView>] [-Options <RegistryOptions>] [-ACL <RegistrySecurity>] [<CommonParameters>]
```

## DESCRIPTION
The New-RegKey cmdlet creates new registry keys.  
If a key already exists the key will be written to the pipeline the same way it would with non-existing keys.  
A custom ACL can be set for newly created keys but it will not change the ACL for keys that already exist.  
Keys can be created as volatile, meaning that they will be deleted when the computer is shutdown or restarted.

## EXAMPLES

### Example 1 Create a new registry key
```powershell
PS C:\> New-RegKey -Path HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware
```

Create the new registry key "HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware".

### Example 2 Create a new volatile registry key
```powershell
PS C:\> New-RegKey -Path HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware\TempKey -Options Volatile
```

Create a volatile key "HKEY_LOCAL_MACHINE\SOFTWARE\SomeSoftware\TempKey" that will be deleted when the registry hive is unloaded (when the computer is shutdown/rebooted).

## PARAMETERS

### -Path
The full path to the key that should be created.

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
The computer where the registry key should be created.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: PSComputerName

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeyPermissionCheck
Specifies whether security checks are performed when opening registry keys and accessing their name/value pairs.  
Default: The registry key inherits the mode of its parent. Security checks are performed when trying to access subkeys or values, unless the parent was opened with ReadSubTree or ReadWriteSubTree mode.  
ReadSubTree: Security checks are not performed when accessing subkeys or values. A security check is performed when trying to open the current key, unless the parent was opened with ReadSubTree or ReadWriteSubTree.  
ReadWriteSubTree: Security checks are not performed when accessing subkeys or values. A security check is performed when trying to open the current key, unless the parent was opened with ReadWriteSubTree.

```yaml
Type: RegistryKeyPermissionCheck
Parameter Sets: (All)
Aliases:
Accepted values: Default, ReadSubTree, ReadWriteSubTree

Required: False
Position: Named
Default value: Default
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

### -Options
Allows you to specify that the key is "Volatile" meaning that it will be deleted when the registry hive is unloaded.

```yaml
Type: RegistryOptions
Parameter Sets: (All)
Aliases:
Accepted values: None, Volatile

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```



### -ACL
Specifies a custom ACL to set on the newly created key.  
Note: If the key already exists the ACL will not change.

```yaml
Type: RegistrySecurity
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```




### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### Microsoft.Win32.RegistryKey

## NOTES

## RELATED LINKS
