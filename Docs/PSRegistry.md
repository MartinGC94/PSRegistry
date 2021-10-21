---
Module Name: PSRegistry
Module Guid: ea7fa35e-49f6-4594-a7e4-344984da0523
Download Help Link:
Help Version: 1.0
Locale: en-US
---

# PSRegistry Module
## Description
This module serves as an alternative to the registry provider included in Powershell that gives you more flexibility in how you access registry keys.  
Almost all of the included cmdlets include built-in remoting through the remote registry services.  
For cmdlets without a ComputerName parameter you can use Get-RegKey to get a handle to a remote registry key that you can then pass into the cmdlets as needed.

## PSRegistry Cmdlets
### [Add-RegKeyProperty](Add-RegKeyProperty.md)
Creates or updates registry key properties.

### [Copy-RegKey](Copy-RegKey.md)
Copies a registry key (with subkeys and values) to another key.

### [Dismount-RegHive](Dismount-RegHive.md)
Dismounts a mounted registry hive.

### [Get-RegKey](Get-RegKey.md)
Gets registry key(s) along with their properties.

### [Mount-RegHive](Mount-RegHive.md)
Mounts registry hive files as keys in the registry.

### [New-RegKey](New-RegKey.md)
Creates new registry keys.

### [Remove-RegKey](Remove-RegKey.md)
Deletes registry keys.

### [Remove-RegKeyProperty](Remove-RegKeyProperty.md)
Deletes registry properties.

### [Rename-RegKey](Rename-RegKey.md)
Renames registry keys.
