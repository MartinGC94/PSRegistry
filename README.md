# PSRegistry

This module serves as an alternative to the registry provider included in Powershell that lets you do things that the original provider doesn't allow.
Examples include:

* Accessing registry keys with different RegistryRights and PermissionsChecks (Allowing you to take ownership of keys the original provider can't touch)
* Improved performance
* Remote management without WinRM
* Easier syntax (no need to create PS drives for HKU and other registry hives, allows both the full and short names for hives like HKLM or HKEY_LOCAL_MACHINE)
* Ability to mount offline registry hives.


## Examples

```powershell
#Add 2 properties to a key. Argument transformation is used to convert the string and hashtable into their correct types.
Add-RegKeyProperty -Key "HKEY_CURRENT_USER\Console" -Property @{
    QuickEdit=1
    LineWrap=1
}

#Mount the default user registry hive, change the quick edit mode for the console and dismount the hive again.
Mount-RegHive -Path "C:\Users\Default\NTUSER.DAT" -DestinationPath HKLM:\TempMount
Add-RegKeyProperty -Key HKLM:\TempMount\Console -Name QuickEdit -Value 1 -ValueKind DWord
Dismount-RegHive -Path HKLM:\TempMount

#Copy the current user console settings to a different user
Copy-RegKey -Key HKCU:\Console -Destination "HKEY_USERS\S-1-5-21-735598605-1552820368-3346217319-1001\Console"

```