---
external help file: PSRegistry.dll-Help.xml
Module Name: PSRegistry
online version: 1.0.0
schema: 2.0.0
---

# Get-RegKey

## SYNOPSIS
Gets registry key(s) along with their properties.

## SYNTAX

```
Get-RegKey [-Path] <String[]> [[-ComputerName] <String[]>] [-Recurse] [-Depth <Int32>] [-KeyOnly]
 [-KeyPermissionCheck <RegistryKeyPermissionCheck>] [-Rights <RegistryRights>] [-View <RegistryView>]
 [-ValueOptions <RegistryValueOptions>] [<CommonParameters>]
```

## DESCRIPTION
The Get-RegKey cmdlet retrieves registry keys and properties.  
The command has several options for how it should read the registry keys that affects performance and what you can do with the registry key objects returned by the cmdlet.  
The default values for the various options are optimized for making changes to registry keys.

## EXAMPLES

### Example 1 Get a registry key and save it in a variable
```powershell
PS C:\> $ConsoleKey=Get-RegKey -Path HKCU:\Console
```

Returns the "HKCU:\Console" key with write permissions.

### Example 2 Get a key and its subkeys up to a specific depth without their properties
```powershell
PS C:\> $ConsoleKeys=Get-RegKey -Path HKCU:\Console -KeyOnly -Recurse -Depth 2
```

Returns the "HKCU:\Console" key and subkeys up to 2 levels down from the "HKCU:\Console" key.  
The KeyOnly parameter makes the command skip retrieving the properties from each key.

### Example 3 Get a registry key with only the permissiones required to read the key and properties
```powershell
PS C:\> $ConsoleKeys=Get-RegKey -Path HKCU:\Console -Recurse -KeyPermissionCheck ReadSubTree -Rights EnumerateSubKeys,QueryValues
```

Returns the "HKCU:\Console" key and subkeys with the minimum required permissions required for reading.

### Example 4 Take ownership of a key and grant yourself full control permissions
```powershell
PS C:\> $RegKey=Get-RegKey -Path 'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\AlternateShells' -KeyOnly -KeyPermissionCheck ReadWriteSubTree -Rights TakeOwnership
$ACL=$RegKey.GetAccessControl()
$ACL.SetOwner([System.Security.Principal.NTAccount]::new("$env:USERNAME"))
$RegKey.SetAccessControl($ACL)
$RegKey.Dispose()
$RegKey=Get-RegKey -Path 'HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\AlternateShells' -KeyOnly -KeyPermissionCheck ReadWriteSubTree -Rights ChangePermissions
$ACL.AddAccessRule([System.Security.AccessControl.RegistryAccessRule]::new($env:USERNAME,"FullControl",("ContainerInherit","ObjectInherit"),"none","Allow"))
$RegKey.SetAccessControl($ACL)
$RegKey.Dispose()
```

Takes ownership of a key and grants the user full control.  
Note that the powershell process needs to have the SeTakeOwnershipPrivilege enabled for this to work.

## PARAMETERS

### -Path
Specifies the path to get the registry keys from.

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
The computername to get the key from.

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

### -Recurse
Specifies if the cmdlet should get subkeys from each path specified.

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

### -Depth
Specifies how deep the command should look for keys when the Recurse parameter is specified.  
The depth is relative to each input path.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeyOnly
Skip getting the properties for each key.

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



### -Rights
Specifies the rights used when accessing each key.  
The default rights allow you to read/write registry keys.

```yaml
Type: RegistryRights
Parameter Sets: (All)
Aliases:
Accepted values: QueryValues, SetValue, CreateSubKey, EnumerateSubKeys, Notify, CreateLink, Delete, ReadPermissions, WriteKey, ExecuteKey, ReadKey, ChangePermissions, TakeOwnership, FullControl

Required: False
Position: Named
Default value: EnumerateSubKeys,QueryValues,Notify,SetValue
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

### -ValueOptions
Specifies options for how property values should be read.

```yaml
Type: RegistryValueOptions
Parameter Sets: (All)
Aliases:
Accepted values: None, DoNotExpandEnvironmentNames

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
