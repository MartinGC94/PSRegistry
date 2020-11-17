# PSRegistry

This module serves as an alternative to the registry provider included in Powershell that lets you do things that the original provider doesn't allow.
Examples include:

* Accessing registry keys with different RegistryRights and PermissionsChecks (Allowing you to take ownership of keys the original provider can't touch)
* Improved performance
* Remote management without WinRM
* Easier syntax (no need to create PS drives for HKU and other registry hives, allows both the full and short names for hives like HKLM or HKEY_LOCAL_MACHINE)
* (TODO:) Ability to mount offline registry hives.
* (TODO:) ability to import/export registry keys in the common .reg file format


## Usage

There are many configuration options, see the options to `Set-PSReadLineOption`.  `PSReadLine` has help for it's cmdlets as well as an `about_PSReadLine` topic - see those topics for more detailed help.

To set your own custom keybindings, use the cmdlet `Set-PSReadLineKeyHandler`. For example, for a better history experience, try:

```powershell
Set-PSReadLineKeyHandler -Key UpArrow -Function HistorySearchBackward
Set-PSReadLineKeyHandler -Key DownArrow -Function HistorySearchForward
```