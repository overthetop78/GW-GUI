# User guide — GW GUI

![GW GUI main window — Read tab](images/main-read-en.png)

## First-time setup

1. Open **Options → Preferences**.
2. Detect or select `gw.exe`. Downloading Host Tools from the application is always an explicit action.
3. Under **Hardware**, scan for the Greaseweazle controller and describe each connected drive.
4. Choose the default image folder, language and theme.

Disconnected controllers and drives remain registered. Scan again only when their port or configuration changes.

## Read

- **Raw SCP image** archives the disk flux directly.
- **Known disk format** decodes to ADF, ST, IMA or another compatible container.
- The filename field never contains the extension.
- Automatic numbering supports numbers or letters and advances only after a successful read.
- Technical controls are folded under **Advanced settings**.
- The **Default** profile disables every optional setting. Saving creates a profile belonging only to Read.

When a file exists, explicitly choose overwrite, the next available number, or return to edit the name.

## Write

Select a source image. GW GUI detects its format from the container and size when reliable; ambiguity must be resolved manually. A mandatory summary shows the file, format, drive and verification state before writing. Disabling verification is an advanced option highlighted as unsafe.

## Convert

Format checkboxes handle both single and multiple conversion. Outputs incompatible with the source are disabled. With no explicit extension checked, the normal extension is used; selecting one or more extensions replaces that implicit choice. Selected formats remain pinned at the top.

Tags such as `[PC-720]` and `[AMIGA-DD]` prevent collisions in multi-conversion. Each output runs separately, and the final report preserves successful outputs when another conversion fails.

## SCP visualization

Open an SCP capture to display both sides, zoom, pan and select a track. The inspector shows revolutions, estimated speed, checksum and structures recognized by the automatic or manually selected decoder.

## Tools, diagnostics and hardware

- **Tools** contains disk erase and head cleaning, both confirmed before execution.
- **Options → Diagnostics** contains information, USB bandwidth, RPM and head seek.
- **Options → Hardware** contains pins, reset, delays and firmware.

Potentially dangerous actions stay in dialogs away from daily workflows.

## Console, stop and history

The exact command and `gw` output appear inside the lower console, which can be hidden or exported. **Execute** becomes **Stop** during an operation and asks for confirmation. Operation history under Options retains up to ten 5 MiB log files.

## Data and portable mode

- Installed mode: data is stored in Windows user folders.
- Portable ZIP: `portable.flag` places settings, logs and managed Host Tools in the adjacent `Data` directory.

GW GUI sends no telemetry.
