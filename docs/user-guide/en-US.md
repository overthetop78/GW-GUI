# GW GUI User Guide

GW GUI is a Windows application for reading, writing, converting, inspecting, and emulating floppy-disk images. It can control Greaseweazle hardware, work with disk-image files through its internal engine, and run saved emulated-machine configurations.

This guide describes the English interface shown in the current version of the application. It is written as the source of the printable user manual: screenshots illustrate the controls, while the surrounding text explains what to choose, why to choose it, and how to verify the result.

> **Important:** Reading a disk is non-destructive. Writing, erasing, firmware updating, and some hardware tools can modify media or hardware. Read the warning attached to the relevant procedure before clicking **Execute**.

### How to use this guide

If this is your first time using GW GUI, complete [Getting started](#getting-started), then follow [Reading a disk](#reading-a-disk). If the application is already configured, go directly to the chapter for the operation you want to perform. The options chapters serve as a reference when a procedure asks you to change a drive, engine, profile, or emulated-machine setting.

Interface names are shown in **bold**. Filenames, paths, commands, and literal values are shown as `code`. Notes explain normal behaviour; warnings identify operations that may alter a disk, controller, or stored configuration.

## Contents

1. [Understanding the workflow](#understanding-the-workflow)
2. [Getting started](#getting-started)
3. [Main window](#main-window)
4. [Reading a disk](#reading-a-disk)
5. [Writing a disk](#writing-a-disk)
6. [Converting disk images](#converting-disk-images)
7. [Visualizing a disk image](#visualizing-a-disk-image)
8. [Exploring disk contents](#exploring-disk-contents)
9. [Using the tools](#using-the-tools)
10. [Emulation](#emulation)
11. [Application options](#application-options)
12. [Emulation options](#emulation-options)
13. [Amiga configuration](#amiga-configuration)
14. [Hardware diagnostics and maintenance](#hardware-diagnostics-and-maintenance)
15. [Logs and operation history](#logs-and-operation-history)
16. [Application data and portable use](#application-data-and-portable-use)
17. [Recommended workflows](#recommended-workflows)
18. [Safety checklist](#safety-checklist)
19. [Troubleshooting](#troubleshooting)
20. [Glossary](#glossary)
21. [Quick reference](#quick-reference)

## Understanding the workflow

GW GUI separates physical-disk operations from image-file operations:

| Goal | Input | Output | Recommended page |
|---|---|---|---|
| Preserve a floppy disk | Physical disk | Image file | **Read** |
| Recreate a floppy disk | Image file | Physical disk | **Write** |
| Change image format | Image file | One or more image files | **Conversion** |
| Inspect tracks and anomalies | Image file | Visual analysis | **Visualization** |
| Browse files stored in an image | Supported image/file system | Files and directories | **Disk Explorer** |
| Diagnose a drive or controller | Greaseweazle hardware | Measurements or status | **Tools** |
| Run a saved virtual machine | Saved machine configuration | Emulation session | **Emulation** |

For preservation, first make a raw capture and keep it unchanged as a master. Create converted or repaired working copies from that master. This avoids repeating a physical read and preserves information that a sector-based format may not retain.

## Getting started

### Requirements

- Windows with the Microsoft .NET Desktop Runtime required by the application.
- A Greaseweazle controller for physical floppy-disk operations.
- A configured path to `gw.exe` when using the Greaseweazle Host Tools engine.
- Legally obtained ROM files when an emulated machine requires them.

The application checks its required .NET runtime at startup. If it is missing, follow the installation prompt, then restart GW GUI.

### Before connecting hardware

Check the following before running a physical-disk operation:

1. Connect the Greaseweazle controller to a stable USB port.
2. Connect the floppy cable with the correct orientation.
3. Connect the drive power supply before inserting valuable media.
4. Confirm that the drive size and density match the disk.
5. Write-protect the source disk when possible.

GW GUI cannot prevent damage caused by incorrect cabling, unsuitable power, or a mechanically unsafe drive. Test unfamiliar hardware with an expendable disk first.

### First launch

1. Open `gwgui.exe`.
2. Open **Options**.
3. In **Controllers and drives**, scan for the controller and configure the drive.
4. Verify or select the path to `gw.exe`.
5. In **Engines**, choose which engine should perform each operation.
6. Return to the main window and select the required operation tab.

### Confirming that setup is ready

A working setup should show the controller and drive in the status bar, for example a drive number, size, density, and COM port. In **Options > Controllers and drives**, the controller should be marked **Available** and the drive **Configured**. Run **Controller information** before reading valuable media if you want to verify communication without altering a disk.

### Choosing an engine

GW GUI can expose more than one implementation for some operations. The **Greaseweazle Host Tools** engine invokes the configured `gw.exe`; the internal GW GUI engine handles supported operations inside the application. Engine selection is explicit and independent for reading, writing, conversion, and Disk Explorer. If an operation is unsupported by the selected engine, GW GUI reports that condition instead of changing engines automatically.

## Main window

The main window groups the principal operations into seven tabs:

- **Read** creates an image from a physical disk.
- **Write** writes an image to a physical disk.
- **Conversion** converts one disk-image format into one or more output formats.
- **Visualization** displays tracks and flux or decoded data.
- **Disk Explorer** browses supported file systems and disk contents.
- **Tools** provides hardware maintenance and diagnostic commands.
- **Emulation** manages and runs saved emulated machines.

The console at the bottom displays the command being executed and its output. The status bar reports the selected drive, profile, and current state.

### Reading the interface

Most operation pages follow the same pattern:

1. **Source or destination** controls identify the disk, image, or folder.
2. **Format controls** select automatic detection or an explicit machine and format.
3. **Profile controls** apply reusable settings.
4. **Advanced settings** expose parameters that are normally optional.
5. **Execute** starts the operation.
6. The **console** shows the generated command, progress, warnings, and errors.

The **Execute** button does not imply that all values are safe for the inserted disk. Always review the destination and selected drive before a write or maintenance operation.

### Status bar and console

The left side of the status bar identifies the active physical drive. The centre shows the active profile when one is selected. The state indicator reports whether the application is ready or busy. The console is not merely diagnostic: it is the authoritative record of the command sent to the selected engine. Use its copy control when you need to preserve or share that command.

## Reading a disk

Open the **Read** tab to capture a physical floppy disk as an image.

<p align="center"><img src="images/main-read-en.png" alt="Read tab" width="78%"></p>

### Basic procedure

1. Insert the source disk in the configured drive.
2. Choose the image type:
   - **Raw image (SCP)** preserves flux-level information.
   - **Known disk format** creates an image using a selected machine and format.
3. Choose the destination folder.
4. Enter the output filename.
5. Select a profile if required.
6. Click **Execute**.

The console shows the exact command and progress. Do not remove the disk or disconnect the controller until the operation has finished.

### Choosing the output type

Use **Raw image (SCP)** when the objective is archival capture, analysis, recovery, or later conversion. A raw image records timing information and multiple revolutions, which is useful for unusual formats, weak sectors, protection schemes, and damaged media.

Use **Known disk format** when you already know the disk family and need a directly usable sector image. This choice may be smaller and easier to open in other software, but it represents the decoded result rather than every detail observed by the drive.

When uncertain, create the raw image first. You can convert it later without reading the disk again.

### Folder, filename, and profile

The **Folder** is the destination directory. The **Filename** should identify the disk without relying only on its physical label. A useful archival name contains the title, disk number or side, and a condition note when applicable. Do not add a format extension that conflicts with the selected output format.

A **Profile** applies a saved set of read parameters. Select one only when you know what it contains. The **Default** profile is appropriate for a normal first attempt; a specialised recovery profile may deliberately read more revolutions or a different track range and therefore take longer.

### Advanced settings

Expand **Advanced settings** to access format-specific or expert parameters. Leave these values unchanged unless the disk requires a particular track range, revolution count, or controller option.

Common advanced values include:

| Setting | Purpose | When to change it |
|---|---|---|
| Track range | Limits the cylinders and heads to read | Single-sided media, unusual geometry, or a targeted recovery pass |
| Revolutions | Controls how many rotations are sampled | Increase for unstable or protected tracks; reduce only for speed when appropriate |
| Expert arguments | Passes additional engine parameters | Only when following documented Greaseweazle guidance |

### Verifying a successful read

Do not rely only on the absence of an error dialog. After the command completes:

1. Confirm that the output file exists and is not empty.
2. Read the final console lines for failed or missing tracks.
3. Open the image in **Visualization** to check that both sides and the expected track range contain data.
4. Open it in **Disk Explorer** when the file system is supported.
5. Keep the operation log with important archival captures.

If repeated reads differ, preserve each raw capture rather than overwriting the first one. Differences can be useful during recovery.

## Writing a disk

Open the **Write** tab to write an existing image to a physical floppy disk.

<p align="center"><img src="images/main-write-en.png" alt="Write tab" width="78%"></p>

### Basic procedure

1. Insert the destination disk.
2. Select the source image with **Browse**.
3. Confirm the detected format.
4. Select a profile if required.
5. Click **Execute**.

Writing replaces data on the destination disk. Verify the selected drive and image before starting.

> **Warning:** Writing is destructive. It replaces magnetic data on the destination disk. Use a write-protected source archive and a separate destination disk whenever possible.

### Before writing

Check four items before clicking **Execute**:

1. **Image:** the selected path is the intended source image.
2. **Disk:** the disk in the drive may safely be overwritten.
3. **Drive:** the configured size and density suit the destination medium.
4. **Format:** automatic detection or the manually selected format matches the image.

If the source image has not been tested, open it in **Visualization** or **Disk Explorer** first. A successful write cannot repair an incomplete source image.

### Track inspection and modification

After an image is selected, **Visualize tracks** opens its track representation. **Modify** exposes the supported image modifications before writing. Available actions depend on the selected format and engine.

### Verifying a written disk

When the engine supports verification, use it for important media. Otherwise, read the written disk back to a new image and compare its decoded contents or inspect it in **Visualization**. Keep the verification capture separate from the original image so that the original is never overwritten.

If writing fails at consistent tracks, check disk condition, density, drive cleanliness, and drive configuration. If failures occur randomly, check USB stability and controller communication.

## Converting disk images

The **Conversion** tab converts a source image into one or several destination formats.

<p align="center"><img src="images/main-conversion-en.png" alt="Conversion tab" width="78%"></p>

### Basic procedure

1. Select the source image.
2. Optionally provide output names.
3. Choose a machine family.
4. Select one or more output formats and extensions.
5. Enable **Add tags** if filenames should use the configured tag pattern.
6. Click **Execute**.

The **Selected** panel lists the requested outputs. **File migration** provides the dedicated workflow for migrating supported files rather than performing a standard image conversion.

### Selecting formats

The **Machine** list filters the formats shown in the **Format** panel. A format name describes the logical disk layout; the extension describes the output container. Some formats can be represented by more than one extension, and some containers cannot preserve every feature of a raw source.

Select only outputs you actually need. Multiple formats are useful when creating an archival master, an emulator-compatible copy, and a copy for another analysis tool in one operation.

### Output naming and tags

**Output names** lets you control the base names generated for selected formats. **Add tags** applies the filename pattern configured in **Options > General**. Tags can encode family, format, extension, date, or time. Preview the example in Options before converting a large batch so that files are named consistently.

### Checking conversion results

For each requested output:

1. Confirm that a file was created.
2. Check the console for tracks or sectors that could not be decoded.
3. Open the result in **Disk Explorer** if it contains a supported file system.
4. Compare the expected disk capacity and contents with the source.

A conversion can complete while reporting information loss that is inherent to the destination format. Retain the original raw image even when the converted image appears correct.

## Visualizing a disk image

The **Visualization** tab displays the structure and data distribution of an image.

<p align="center"><img src="images/main-visualization-en.png" alt="Visualization tab" width="78%"></p>

1. Click **Open a disk image**.
2. Keep **Automatic detection** enabled, or select the machine and format manually.
3. Use **Link zoom** to keep both sides at the same zoom level.
4. Use **Reset** to restore the initial view.
5. Open **Inspector** for detailed information about the selected region.

The legend distinguishes normal flux, short and long transitions, headers, decoded data, and detected anomalies. A raw image may contain data that cannot be decoded into a known file system but can still be inspected here.

### Interpreting the view

Each large circular panel represents one disk side. The centre identifies the side and its current data state; concentric positions correspond to tracks. Colours classify the detected regions according to the legend. The visualizer is intended to answer questions such as:

- Does the image contain data on one side or both?
- Are the expected tracks present?
- Are anomalies isolated or repeated across the disk?
- Did automatic detection identify a plausible machine and format?

An anomaly colour is a reason to inspect the region, not proof that the disk is unusable. Copy protection, non-standard formatting, a weak recording, and a damaged sector can produce different structures that require contextual interpretation.

### Recommended inspection sequence

Start with linked zoom enabled to compare both sides at the same scale. Select a suspicious region, open **Inspector**, and compare it with neighbouring tracks. If the result appears to be a detection problem, disable automatic detection and choose a known machine and format. Return to automatic detection after the test so a forced setting is not accidentally used for another image.

## Exploring disk contents

The **Disk Explorer** tab browses supported disk images as a file hierarchy.

<p align="center"><img src="images/main-disk-explorer-en.png" alt="Disk Explorer tab" width="78%"></p>

1. Open an existing image or read a disk.
2. Keep **Automatic detection** enabled unless you need to force a machine or format.
3. Review the volume information: system, protection, file system, capacity, free space, and item count.
4. Browse directories in the left panel.
5. Select an item to view its details in the right panel.

If the image format or file system is unsupported, use **Visualization** to inspect the raw structure instead.

### Understanding the panels

The top summary describes the mounted image and detected volume. The lower-left panel contains the directory hierarchy. The central table lists items in the selected directory with name, modification date, type, and size. The right panel shows details for the selected item.

Disk Explorer does not imply that every raw track was decoded perfectly. Use the volume summary and item count as a quick plausibility check, then open representative files or compare them with a known directory listing when preservation accuracy matters.

### When nothing appears

First confirm that the image path is correct. Then check the detected machine and format. A valid image may contain an unsupported or damaged file system, in which case the explorer can remain empty even though **Visualization** shows recorded data. Do not overwrite or discard the source image based only on an empty explorer.

## Using the tools

The **Tools** tab groups Greaseweazle maintenance operations.

<p align="center"><img src="images/main-tools-en.png" alt="Tools tab" width="78%"></p>

Select a command from the list on the left, review its parameters, then click **Execute**. Destructive or hardware-changing commands should only be used after verifying the selected controller and drive.

Most tool dialogs contain three areas: parameters at the top, a status and raw-output area in the centre, and the generated command at the bottom. The command preview changes as options are enabled. An unchecked parameter normally means “do not modify this value,” whereas a checked parameter includes that value in the command.

The individual diagnostic dialogs are described in [Hardware diagnostics and maintenance](#hardware-diagnostics-and-maintenance).

## Emulation

### Opening a saved machine

The **Emulation** tab lists saved configurations. Select one and click **Open**. Each running machine appears in its own tab.

<p align="center"><img src="images/main-emulation-welcome-en.png" alt="Emulation welcome screen" width="78%"></p>

Create and edit machines in **Options > Emulation > Configurations** and **Options > Emulation > Amiga**.

If no configuration appears, create one in Options first. A saved configuration combines the machine model, emulator version, ROM, memory, video, audio, storage, and input mappings. Saving a configuration does not start it; return to the main **Emulation** tab and click **Open**.

### Running-machine controls

<p align="center"><img src="images/main-emulation-running-en.png" alt="Running emulated machine" width="78%"></p>

The running-machine toolbar provides power, pause, reset, save-state, load-state, capture, and display controls. It also shows:

- the configured quick-save and quick-load shortcuts;
- the active renderer, such as Direct3D 11;
- the fullscreen and mouse-release shortcuts;
- audio, controller, and mouse state;
- the current resolution, refresh rate, and frame rate.

The disk strip at the bottom of the emulation display manages removable media for each emulated drive. Keyboard assignments can be changed in **Options > Emulation > Shortcuts**, while emulated keyboard, mouse, and controller mappings are configured in the corresponding Amiga tabs.

### Toolbar reference

| Control group | Purpose |
|---|---|
| Power and pause | Starts, stops, pauses, or resumes the emulated machine |
| Reset controls | Performs the configured soft or hard reset action |
| State controls | Saves or loads an emulator state for rapid continuation |
| Capture | Saves an image of the emulated display |
| Display | Changes the display presentation or enters fullscreen |
| Quick-state reminder | Shows the active save/load shortcuts |
| Renderer | Reports the active video backend |
| Input reminder | Shows fullscreen and mouse-release shortcuts |
| Device indicators | Reports audio, controller, and mouse state |
| Performance | Reports output size, refresh frequency, and frame rate |

### Leaving fullscreen or releasing the mouse

The toolbar displays the currently assigned keys. In the illustrated configuration, **Alt+Return** toggles fullscreen and **F12** releases the mouse. Treat the displayed values as authoritative because shortcuts can be reassigned.

### Using floppy media

The drive strip identifies each emulated drive, such as `DF0:`. Use its media controls to insert, replace, or eject an image. Replacing media changes only the running machine’s inserted disk; it does not change the storage-device definition in the saved machine unless that action is explicitly saved.

## Application options

Open **Options** from the main window to configure the application.

### General

<p align="center"><img src="images/options-general-en.png" alt="General options" width="72%"></p>

The **General** tab contains:

- the default disk-image folder;
- interface language and theme;
- filename-tag generation for conversions;
- predefined and recent custom tag patterns;
- a live filename example.

Tag variables include the source name, family, format, extension, date, and time. Use the reset button to restore the default pattern.

The filename preview updates before any files are created. Use it to detect duplicated separators, missing extensions, or ambiguous names. Recent custom patterns provide quick access to earlier naming schemes without replacing the current preset.

### Logs

<p align="center"><img src="images/options-logs-en.png" alt="Log options" width="72%"></p>

Logging can be configured independently for each operation. For every category, choose whether to save logs, set a maximum file size, and decide whether previous logs should be retained. A size of `0` means unlimited. **Open folder** opens the current log directory.

Enable **Keep previous logs** for preservation and diagnostic work where the history of several attempts matters. Disable it when only the most recent result is useful. Maximum size limits apply to log storage, not to captured disk images.

### Controllers and drives

<p align="center"><img src="images/options-controllers-and-drives-en.png" alt="Controllers and drives" width="72%"></p>

Use this tab to:

- scan for connected controllers;
- add and remove drive configurations;
- select drive size, density, and speed;
- save hardware settings;
- choose or automatically find `gw.exe`;
- check for and download Greaseweazle Host Tools updates;
- restore a previously configured executable path.

Saved hardware settings remain available when a drive is temporarily disconnected.

#### Adding a drive

1. Click **Scan** and wait for connected controllers to appear.
2. Click **Add a drive** if the required drive is not already listed.
3. Select its logical drive number, physical size, recording density, and rotation speed.
4. Save the row.
5. Confirm that it shows **Available** and **Configured**.

Use the trash control only to remove the saved configuration; it does not disconnect hardware. If the same controller appears on a different COM port later, scan again before assuming that the stored port is still valid.

#### Managing Greaseweazle Host Tools

**Find gw.exe** searches known locations. **Choose** selects a specific executable. **Check for updates** queries available versions without replacing the installed one. **Download latest version** installs the selected current package, and **Use previous path** restores the earlier configured location. After changing the executable, run **Controller information** to confirm that the selected version can communicate with the controller.

### Engines

<p align="center"><img src="images/options-engines-en.png" alt="Engine selection" width="72%"></p>

Choose the engine independently for reading, writing, conversion, and Disk Explorer. The selected engine is used strictly: if it cannot perform the requested operation, GW GUI reports the limitation instead of silently switching engines.

This independence is intentional. For example, physical reads may use Greaseweazle Host Tools while image conversion and exploration use the internal engine. Record engine choices in a profile or project note when reproducibility matters.

### Profiles

<p align="center"><img src="images/options-profiles-en.png" alt="Profiles" width="72%"></p>

Profiles store reusable settings for read, write, and conversion operations. Select the relevant category to manage its profiles. A selected profile is shown in the main-window status bar and in operation screens.

Use profiles for repeatable workflows rather than as unexplained collections of expert flags. Give each profile a purpose-specific name, such as a particular drive, disk family, or recovery method. Review a profile after updating the underlying engine because supported options can change.

## Emulation options

The **Emulation** options contain general storage settings, global shortcuts, saved configurations, and machine-specific settings.

### General emulation folders

<p align="center"><img src="images/options-emulation-general-en.png" alt="General emulation options" width="72%"></p>

Set the shared emulation storage folder and the default folders for captures and saved states. **Open folder** opens the shared location in File Explorer.

Keep captures and saved states in separate folders. A capture is an ordinary image; a saved state contains emulator-specific machine state and may depend on the emulator version and configuration that created it. Back up configuration and media alongside important saved states.

### Global shortcuts

<p align="center"><img src="images/options-emulation-shortcuts-en.png" alt="Emulation shortcuts" width="72%"></p>

Search for an action or key assignment, assign or remove shortcuts, restore defaults, and clear conflicts. The status column identifies valid and conflicting assignments.

To change a shortcut, find the action, click **Assign**, and press the desired key combination. Check the status before closing Options. **Clear conflicts** removes conflicting assignments; it does not restore the default mapping. Use **Restore defaults** when you want to replace custom assignments with the standard set.

### Saved configurations

<p align="center"><img src="images/options-emulation-configurations-en.png" alt="Saved emulation configurations" width="72%"></p>

This page lists saved machines. Select a configuration to edit it in the **Amiga** tab. You can refresh the list or delete the selected configuration.

Deleting a configuration removes the saved machine definition. It should not be used as a way to eject media or close a running machine. Before deletion, note any ROM, hard-disk image, and state files associated with the configuration.

## Amiga configuration

The current interface provides detailed Amiga configuration pages. The same settings structure can be extended for other emulated systems without changing the main workflow.

### General

<p align="center"><img src="images/options-amiga-general-en.png" alt="Amiga general settings" width="72%"></p>

Choose the Amiga model, save the configuration, install or replace the emulator version, and define default folders for hard disks and other media. **Search versions** queries the official emulator-version source.

Start with the model because it constrains later pages. Changing it can alter the available CPU, memory, ROM, chipset, and storage choices. After selecting an emulator version, save the configuration before launching it from the main window. Installing another emulator version replaces the version used by that configuration; it does not create a second copy of the machine.

### CPU

<p align="center"><img src="images/options-amiga-cpu-en.png" alt="Amiga CPU settings" width="72%"></p>

The CPU page shows the processor selected by the machine model and provides compatible precision, FPU, and speed choices. Options that do not apply to the selected model remain disabled.

- **CPU model** identifies the emulated processor.
- **Precision** controls the timing model. Cycle-exact modes favour hardware compatibility but require more host processing.
- **FPU** enables a compatible floating-point unit when supported.
- **CPU speed** selects original timing or an accelerated mode.

For a baseline configuration, keep the model-derived CPU and original speed. Change acceleration only after the machine boots correctly at its standard settings.

### RAM

<p align="center"><img src="images/options-amiga-ram-en.png" alt="Amiga RAM settings" width="72%"></p>

Configure Chip RAM, Slow RAM, Fast RAM, and supported expansion memory. Compatibility messages explain restrictions for the selected machine, and the total configured memory is displayed at the bottom.

**Chip RAM** is accessible to the custom chips and is required by the platform. **Slow RAM** represents compatible expansion memory used by common configurations. **Fast RAM** is processor-oriented expansion memory. **Zorro III RAM** applies only to models that support that expansion architecture. The compatibility messages and disabled controls prevent combinations that the selected model cannot represent.

### ROM

<p align="center"><img src="images/options-amiga-rom-en.png" alt="Amiga ROM settings" width="72%"></p>

Select the system Kickstart ROM, optional extended ROM, and ROM key. The detected-ROM list displays names, revisions, and compatibility with the selected model. Select a detected ROM and click **Use**, or browse to a file manually.

ROM files are not supplied by GW GUI. Use ROMs you are legally permitted to use.

The detected list is preferable to guessing from a filename: it reports the ROM identity and revision and evaluates compatibility with the selected model. **Compatible** is the normal choice; **Partially compatible** indicates that the ROM may boot but does not precisely match the machine. **Refresh** rescans the configured ROM locations. **Use** assigns the selected detected ROM to the configuration.

### Video

<p align="center"><img src="images/options-amiga-video-en.png" alt="Amiga video settings" width="72%"></p>

Configure video standard, aspect ratio, resolution, line mode, border cropping, renderer, color depth, frame skipping, gamma, and flicker fixing. Additional chipset settings are available further down the page when supported by the selected model.

| Setting | Practical effect |
|---|---|
| Video standard | Selects PAL or NTSC timing and expected refresh behaviour |
| Aspect ratio | Controls how the emulated picture is scaled |
| Resolution | Selects automatic or explicit output detail |
| Line mode | Controls treatment of interlaced or line-doubled output |
| Crop borders | Removes unused overscan only when enabled |
| Rendering | Chooses the graphics backend |
| Color depth | Selects output colour precision |
| Frame skip | Reduces rendered frames when enabled |
| Gamma | Adjusts brightness response |
| Flicker fixer | Processes modes that would otherwise visibly flicker |

Change one display setting at a time. If the emulation window becomes blank or unstable, return to automatic resolution, disabled frame skip, neutral gamma, and the previously working renderer.

### Audio

<p align="center"><img src="images/options-amiga-audio-en.png" alt="Amiga audio settings" width="72%"></p>

Enable or disable audio, choose the output device and latency, then configure interpolation, Amiga filtering, filter type, stereo separation, floppy-drive sound, and CD-audio volume.

Lower latency reduces delay but can cause drop-outs on a busy computer. Increase it if audio crackles. Interpolation and the Amiga audio filter change sound reproduction rather than emulated program logic. Drive-sound volume controls the simulated mechanical sound separately from normal Amiga audio.

### Storage

<p align="center"><img src="images/options-amiga-storage-en.png" alt="Amiga storage settings" width="72%"></p>

The storage page lists device identifiers, types, models, associated media, and available actions. Add, configure, or remove devices here. Floppy disks and CDs can be inserted or replaced directly from a running machine.

The **device identifier** is how the emulated system addresses the device. **Type** distinguishes floppy, hard-disk, optical, and other supported devices. **Model** describes the emulated hardware, while **Associated media** identifies the currently assigned image. Configure the device before associating valuable writable media, and keep backups of hard-disk images.

### Keyboard

<p align="center"><img src="images/options-amiga-keyboard-en.png" alt="Amiga keyboard settings" width="72%"></p>

Search Amiga keys and host assignments, assign new keys, remove mappings, restore defaults, or clear conflicts. The status column reports whether each assignment is valid.

The left column names the emulated Amiga key; **Association** shows the host key combination. A valid mapping can still be inconvenient if Windows or the application reserves the same shortcut, so test critical combinations inside the running machine. Avoid assigning the mouse-release or fullscreen shortcut to a key that the emulated software needs frequently.

### Mouse

<p align="center"><img src="images/options-amiga-mouse-en.png" alt="Amiga mouse settings" width="72%"></p>

Set physical mouse speed, choose which analog stick controls the mouse, adjust the analog dead zone and speed, and configure mouse-action mappings. Restore defaults or clear mapping conflicts when necessary.

Increase the dead zone if a controller causes pointer drift. Adjust left- and right-stick speed independently when both sticks are enabled. The lower mapping table associates host inputs with mouse buttons or actions; inspect its conflict status after changing controller mappings elsewhere.

### Controllers

<p align="center"><img src="images/options-amiga-controllers-en.png" alt="Amiga controller settings" width="72%"></p>

Detect connected controllers, assign devices and controller types to Amiga ports, and configure controller mappings and turbo-fire settings. Available choices depend on detected hardware and the selected machine.

Port 1 and Port 2 are configured independently. **Automatic** controller type is a sensible starting point, but software expecting a particular joystick or mouse may require an explicit type. Run detection before assigning a newly connected controller. Turbo fire repeatedly activates a mapped input and should remain disabled unless the game or application benefits from it.

## Hardware diagnostics and maintenance

These dialogs are opened from the **Tools** tab. Each dialog previews the generated Greaseweazle command. Review it before clicking **Execute**.

### Controller information

<p align="center"><img src="images/tool-controller-information-en.png" alt="Controller information" width="62%"></p>

Displays information reported by the selected controller. Expand **Raw output** when you need the complete command response.

Use this as the first diagnostic command. A successful response confirms that GW GUI can start the configured Host Tools executable and communicate with the selected device. Record the firmware and hardware information before performing an update.

### USB bandwidth

<p align="center"><img src="images/tool-usb-bandwidth-en.png" alt="USB bandwidth" width="62%"></p>

Measures the available USB communication bandwidth. Use it to diagnose unstable transfers or an unsuitable USB connection.

Close other software using the controller before testing. Repeat the measurement after changing the USB port, cable, or hub. Compare results under similar conditions rather than treating a single measurement as an absolute guarantee.

### Drive speed

<p align="center"><img src="images/tool-drive-speed-en.png" alt="Drive speed" width="62%"></p>

Measures the drive rotation speed. Increase the number of measurements when you need a more representative result.

A single measurement is a quick check; several measurements reveal whether the speed is stable. Let the drive reach normal speed before interpreting the result. An unexpected value may indicate a wrong configured speed, a mechanical issue, or a measurement setup problem.

### Seek head

<p align="center"><img src="images/tool-seek-head-en.png" alt="Seek head" width="62%"></p>

Moves the drive head to a selected cylinder. **Allow extreme cylinders** permits normally restricted positions, and **Keep motor active** leaves the motor running during the operation. Use extreme positions only when the hardware procedure explicitly requires them.

Normal seeking is useful for confirming head movement or positioning before a diagnostic. Listen for abnormal repeated impacts and stop if the requested cylinder is inappropriate for the drive. This tool does not read or validate data at the destination cylinder.

### Drive alignment diagnostic

<p align="center"><img src="images/tool-drive-alignment-en.png" alt="Drive alignment diagnostic" width="62%"></p>

Runs repeated reads for drive-alignment analysis. It supports track selection, revolution and read counts, decoding format, raw flux, index, speed, PLL, density-pin, hard-sector, TG43, and reverse-data options. Alignment work requires appropriate reference media and hardware knowledge.

Begin with a known reference disk and the smallest set of overrides. **Alternating tracks** defines the tracks and heads sampled; **Revolutions per track** controls each sample duration; **Number of reads** determines repetition. Enable a custom disk definition or decoding format only when it matches the reference media. Options such as fake index, hard sectors, PLL overrides, density pins, and TG43 are hardware- or format-specific and can invalidate a comparison when used incorrectly.

### Hardware pins

<p align="center"><img src="images/tool-hardware-pins-en.png" alt="Hardware pins" width="62%"></p>

Reads or changes a supported controller pin. Select the pin, enable **Change pin** only when writing a value, and select **High level** when required by the intended hardware operation.

With **Change pin** disabled, the command queries the pin. This is the safer default. Changing a level directly affects controller I/O and should be done only with the correct Greaseweazle hardware documentation and attached-drive wiring.

### Reset controller

<p align="center"><img src="images/tool-reset-controller-en.png" alt="Reset controller" width="62%"></p>

Resets the Greaseweazle controller. Use this when the controller is detected but no longer responds normally.

Wait for any active disk operation to finish before resetting. Afterward, scan the controller again if its connection status does not recover automatically. A reset does not repair a wrong `gw.exe` path or a disconnected USB device.

### Delays

<p align="center"><img src="images/tool-delays-en.png" alt="Controller delays" width="62%"></p>

Reads or changes controller timing values, including selection, head step, settle, motor, automatic deselection, write timing, and index mask delays. Enable only the values that you intend to modify.

Unchecked fields leave the corresponding controller value unchanged. Before editing, record the existing values. Timing changes can affect every subsequent physical operation, so test with expendable media and restore known-good values if behaviour becomes unreliable.

### Firmware

<p align="center"><img src="images/tool-firmware-en.png" alt="Firmware update" width="62%"></p>

Updates controller firmware. **Update bootloader** is explicitly marked as risky and should remain disabled unless the official firmware procedure requires it. Do not disconnect the controller during an update.

Before updating, confirm the connected controller with **Controller information**, use a stable direct USB connection, and close other software that could access it. After completion, reconnect or rescan the controller and read its information again to verify the reported firmware version.

## Logs and operation history

Open the operation history to inspect saved logs by operation.

<p align="center"><img src="images/operation-history-en.png" alt="Operation history" width="68%"></p>

Select a log on the left to display its contents. **Export** saves a copy for diagnostics or support. Paths and command lines may contain personal folder names, so review exported logs before sharing them.

The live console in the main window shows the current command and recent output. Its copy button copies the displayed text.

### Reading a log

A useful diagnostic log contains the generated command, timestamps, engine output, and the final status. Work from the bottom upward: identify the final error, then locate the first warning or failed track that preceded it. A later generic failure is often only the consequence of an earlier, more specific message.

When comparing two attempts, check that the controller, drive, engine, profile, source path, output format, and expert arguments were identical. Otherwise, a different result may reflect changed settings rather than disk instability.

## Application data and portable use

GW GUI keeps user data separate from application binaries. Depending on the selected package and mode, settings, logs, downloaded tools, emulator components, captures, states, and machine configurations are stored either in the application `Data` directory or in the configured user-data locations.

Before replacing or moving a portable installation, keep the complete application folder together and back up the `Data` folder. Do not move individual files from `lib`, because the application resolves its own and third-party libraries from that structure.

### Suggested backup contents

Back up the following when they are important to your workflow:

- application settings and profiles;
- controller and drive definitions;
- emulation configurations;
- ROM paths and legally held ROM backups;
- hard-disk and removable-media images;
- captures and saved states;
- operation logs used as preservation records.

Disk images may be much larger than settings. Store archival masters read-only when possible, and work on copies.

## Recommended workflows

### Archiving an unknown disk

1. Inspect and clean the drive using an appropriate maintenance procedure.
2. Write-protect the disk if possible.
3. Select **Read > Raw image (SCP)**.
4. Use a descriptive filename and read the normal track range with multiple revolutions.
5. Review the console and saved log.
6. Inspect both sides in **Visualization**.
7. Convert a copy to likely sector formats.
8. Test the converted copies in **Disk Explorer** or suitable software.
9. Preserve the raw master, log, and notes together.

### Recreating a disk from an image

1. Inspect the image and confirm its expected family and format.
2. Insert an expendable or intentionally writable disk of the correct size and density.
3. Open **Write** and select the image.
4. Confirm the configured drive and detected format.
5. Write the disk.
6. Read it back to a separate verification image.
7. Compare decoded contents and review suspicious tracks visually.

### Creating an emulated Amiga

1. Open **Options > Emulation > Configurations** and create or select a machine.
2. In **Amiga > General**, choose the model and emulator version.
3. Assign a compatible, legally obtained ROM.
4. Keep the model defaults for CPU and RAM on the first boot.
5. Configure video and audio with conservative automatic settings.
6. Add storage devices and associate copied media images.
7. Review keyboard, mouse, and controller assignments.
8. Save the configuration.
9. Return to **Emulation**, select it, and click **Open**.
10. Only after a successful baseline boot, change acceleration or advanced settings one at a time.

## Safety checklist

Before **Read**:

- the source disk is in the correct drive;
- the source is write-protected where possible;
- the output path will not overwrite an existing master;
- the profile and track range match the disk.

Before **Write** or **Erase**:

- the destination disk may be destroyed;
- the image and drive are correct;
- disk size and density are compatible;
- no archival master is being used as the destination.

Before a hardware-changing tool:

- no other operation is running;
- the correct controller is selected;
- current values have been recorded;
- the controller has stable power and USB connectivity;
- the action is supported by the hardware documentation.

## Troubleshooting

### The controller is not listed

1. Reconnect the controller directly to the computer.
2. Open **Options > Controllers and drives**.
3. Click **Scan**.
4. Verify the controller status and drive configuration.
5. Run **Controller information** if detection succeeds but commands fail.

If it still does not appear, try another direct USB port and cable, then rescan. Check Windows Device Manager for a newly detected serial device. A controller visible to Windows but absent from GW GUI usually points to a busy port, stale configuration, or Host Tools problem; a controller absent from Windows points to USB, power, driver, or hardware.

### `gw.exe` cannot be found

Open **Options > Controllers and drives**, then use **Find gw.exe**, **Choose**, or **Download latest version**. Confirm that the detected path points to the intended Greaseweazle installation.

After selecting it, run **Controller information**. If that fails before contacting hardware, inspect the log for an invalid executable path, missing files, or a version that cannot start.

### An operation uses the wrong engine

Open **Options > Engines** and check the engine assigned to that exact operation. GW GUI does not silently fall back to the other engine.

Engine settings are separate: changing the conversion engine does not change reading, writing, or Disk Explorer. Reopen the failing operation after saving the option and confirm the generated command in the console.

### An image is not recognized

Disable automatic detection only if you know the correct machine and format. Otherwise, try the **Visualization** tab to inspect the image at a lower level.

Check whether the source is a raw flux capture, a sector image, a compressed container, or an unrelated file with a misleading extension. Never rename an extension merely to force detection; conversion must interpret the source structure correctly.

### Emulation does not start

Verify the saved configuration, installed emulator version, selected ROM, storage paths, and model compatibility. Review the application log for the complete error details.

Temporarily return CPU, RAM, video, and storage to a simple model-compatible baseline. If the baseline starts, restore one custom setting at a time. A saved state created with another emulator version or machine definition may also fail even when a clean boot works.

### A shortcut or input does not work

Check both the global **Emulation > Shortcuts** page and the machine-specific keyboard, mouse, or controller page. Resolve any assignment marked as conflicting.

If the mouse is captured, use the release shortcut displayed in the running-machine toolbar. If a controller was connected after Options was opened, run controller detection again before assigning it.

### A command fails unexpectedly

1. Read the live console output.
2. Open **Operation history** for the complete saved log.
3. Confirm the selected controller, drive, profile, engine, and file paths.
4. Export the relevant log if it must be shared for diagnosis.

### Audio crackles or pauses

Increase emulation audio latency, close CPU-intensive applications, and return video frame skipping and acceleration to their previous values. Verify that the intended Windows audio device is selected. Change one setting at a time so the effective correction is identifiable.

### The emulation display is blank or slow

Return resolution and line mode to **Automatic**, disable frame skipping and flicker fixing temporarily, and try the previously working renderer. Confirm that the configured ROM and inserted boot media are valid. The FPS indicator helps distinguish a rendering-performance problem from a machine that has simply not booted.

### A read contains unstable tracks

Repeat the read to a new filename, increase revolutions where appropriate, and compare the affected tracks. Clean the drive heads using a correct procedure and inspect the disk for physical damage. Do not repeatedly read visibly shedding or damaged media, because further passes may worsen it.

## Glossary

| Term | Meaning in GW GUI |
|---|---|
| Controller | The Greaseweazle hardware interface connected over USB |
| Drive | The physical floppy drive attached to the controller |
| Engine | The implementation selected to perform an operation |
| Flux | Timing information representing magnetic transitions read from a disk |
| Raw image | A capture retaining low-level disk information, such as SCP |
| Sector image | A decoded representation organised into logical sectors |
| Revolution | One complete rotation sampled while reading a track |
| Cylinder | A radial head position; one cylinder can contain a track on each side |
| Head | The disk side selected by the physical drive |
| Profile | A reusable set of settings for an operation |
| ROM | Firmware image required by an emulated machine |
| Saved state | A snapshot of a running emulator’s machine state |
| Renderer | The graphics backend used to display emulation output |

## Quick reference

| If you want to… | Go to… |
|---|---|
| Preserve a physical disk | **Read** |
| Put an image back on a disk | **Write** |
| Produce another image format | **Conversion** |
| Inspect tracks or flux anomalies | **Visualization** |
| Browse files inside an image | **Disk Explorer** |
| Check controller communication | **Tools > Controller information** |
| Measure drive rotation | **Tools > Drive speed** |
| Review a past command | **Operation history** |
| Configure hardware | **Options > Controllers and drives** |
| Select implementations | **Options > Engines** |
| Create or edit an emulated machine | **Options > Emulation** |
| Start a saved machine | **Emulation** |
