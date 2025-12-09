# SaRA
Microsoft SaRA (Support and Recovery Assistant) – portable, non-expiring build.




This document provides a complete and structured overview of all available command-line switches for the **Microsoft Support and Recovery Assistant Enterprise Edition (SaRA / SaRAcmd.exe)**.
All switches are **case-insensitive**.
Unless marked as optional, switches are **required** to run a scenario.
Multiple optional switches may be combined.

## General Switches

| Switch | Parameter | Description | Required | Scenarios |
|--------|-----------|-------------|----------|-----------|
| `-?` | – | Displays all available SaRAcmd.exe functions and switches. Overrides all other switches. | No | All |
| `-Help` | – | Displays a link to online documentation. Overrides all switches except `-?`. | No | All |
| `-DisplayEULA <file path>` | Path to file | Displays the End User License Agreement (EULA). If a file path is provided, the EULA text is saved to that file. Overrides other switches. | No | All |
| `-S <scenario name>` | Scenario | Specifies the scenario to run. | Yes | All |
| `-AcceptEula` | – | Accepts the EULA. Must be provided before running any scenario. | Yes | All |

## Logging & Output Options

| Switch | Parameter | Description | Required | Scenarios |
|--------|-----------|-------------|----------|-----------|
| `-LogFolder <output path>` | Folder path | Forces SaRAcmd to write scenario-specific logs to the specified output folder. | No | ExpertExperienceAdminTask, OutlookCalendarCheckTask |
| `-HideProgress` | – | Hides the progress display. | No | ExpertExperienceAdminTask, OutlookCalendarCheckTask |

## Outlook & Office Switches

| Switch | Parameter | Description | Required | Scenarios |
|--------|-----------|-------------|----------|-----------|
| `-OfflineScan` | – | Performs an offline Outlook scan while Outlook is running. | No | ExpertExperienceAdminTask |
| `-OfficeVersion <version>` | Version number or `All` | Removes only the specified Office version. Using `All` removes all installed Office versions. | No | OfficeScrubScenario |
| `-RemoveSCA` | – | Removes Shared Computer Activation (SCA) and configures non-SCA activation for Office. | No | OfficeActivationScenario, OfficeSharedComputerScenario |
| `-CloseOffice` | – | Closes all open Office applications. | Yes | OfficeActivationScenario, OfficeSharedComputerScenario, ResetOfficeActivation |
| `-CloseOutlook` | – | Closes Outlook if it is running. | Yes | TeamsAddinScenario |
| `-P <profile name>` | Outlook profile | Specifies the Outlook profile to scan. | No | OutlookCalendarCheckTask |

## Example Usage

### Run a scenario with EULA acceptance
```
SaRAcmd.exe -S OfficeScrubScenario -AcceptEula
```

### Remove all Office versions and output logs
```
SaRAcmd.exe -S OfficeScrubScenario -AcceptEula -OfficeVersion All -LogFolder C:\Logs\SaRA
```

### Run Calendar Check on a specific Outlook profile
```
SaRAcmd.exe -S OutlookCalendarCheckTask -AcceptEula -P "OutlookProfile1"
```

## License
This document covers command-line usage for Microsoft SaRA Enterprise scenarios.
SaRA itself is property of Microsoft Corporation.
