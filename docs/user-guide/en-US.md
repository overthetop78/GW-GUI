# GW GUI User Guide

GW GUI is a Windows application for reading, writing, converting, inspecting, and emulating floppy-disk images. It can control Greaseweazle hardware, work with disk-image files through its internal engine, and run saved emulated-machine configurations.

This guide describes the English interface shown in the current version of the application.

## Contents

1. [Getting started](#getting-started)
2. [Main window](#main-window)
3. [Reading a disk](#reading-a-disk)
4. [Writing a disk](#writing-a-disk)
5. [Converting disk images](#converting-disk-images)
6. [Visualizing a disk image](#visualizing-a-disk-image)
7. [Exploring disk contents](#exploring-disk-contents)
8. [Using the tools](#using-the-tools)
9. [Emulation](#emulation)
10. [Application options](#application-options)
11. [Emulation options](#emulation-options)
12. [Amiga configuration](#amiga-configuration)
13. [Hardware diagnostics and maintenance](#hardware-diagnostics-and-maintenance)
14. [Logs and operation history](#logs-and-operation-history)
15. [Troubleshooting](#troubleshooting)

## Getting started

### Requirements

- Windows with the Microsoft .NET Desktop Runtime required by the application.
- A Greaseweazle controller for physical floppy-disk operations.
- A configured path to `gw.exe` when using the Greaseweazle Host Tools engine.
- Legally obtained ROM files when an emulated machine requires them.

The application checks its required .NET runtime at startup. If it is missing, follow the installation prompt, then restart GW GUI.

### First launch

1. Open `gwgui.exe`.
2. Open **Options**.
3. In **Controllers and drives**, scan for the controller and configure the drive.
4. Verify or select the path to `gw.exe`.
5. In **Engines**, choose which engine should perform each operation.
6. Return to the main window and select the required operation tab.

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

## Reading a disk

Open the **Read** tab to capture a physical floppy disk as an image.

![Read tab](images/main-read-en.png)

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

### Advanced settings

Expand **Advanced settings** to access format-specific or expert parameters. Leave these values unchanged unless the disk requires a particular track range, revolution count, or controller option.

## Writing a disk

Open the **Write** tab to write an existing image to a physical floppy disk.

![Write tab](images/main-write-en.png)

### Basic procedure

1. Insert the destination disk.
2. Select the source image with **Browse**.
3. Confirm the detected format.
4. Select a profile if required.
5. Click **Execute**.

Writing replaces data on the destination disk. Verify the selected drive and image before starting.

### Track inspection and modification

After an image is selected, **Visualize tracks** opens its track representation. **Modify** exposes the supported image modifications before writing. Available actions depend on the selected format and engine.

## Converting disk images

The **Conversion** tab converts a source image into one or several destination formats.

![Conversion tab](images/main-conversion-en.png)

### Basic procedure

1. Select the source image.
2. Optionally provide output names.
3. Choose a machine family.
4. Select one or more output formats and extensions.
5. Enable **Add tags** if filenames should use the configured tag pattern.
6. Click **Execute**.

The **Selected** panel lists the requested outputs. **File migration** provides the dedicated workflow for migrating supported files rather than performing a standard image conversion.

## Visualizing a disk image

The **Visualization** tab displays the structure and data distribution of an image.

![Visualization tab](images/main-visualization-en.png)

1. Click **Open a disk image**.
2. Keep **Automatic detection** enabled, or select the machine and format manually.
3. Use **Link zoom** to keep both sides at the same zoom level.
4. Use **Reset** to restore the initial view.
5. Open **Inspector** for detailed information about the selected region.

The legend distinguishes normal flux, short and long transitions, headers, decoded data, and detected anomalies. A raw image may contain data that cannot be decoded into a known file system but can still be inspected here.

## Exploring disk contents

The **Disk Explorer** tab browses supported disk images as a file hierarchy.

![Disk Explorer tab](images/main-disk-explorer-en.png)

1. Open an existing image or read a disk.
2. Keep **Automatic detection** enabled unless you need to force a machine or format.
3. Review the volume information: system, protection, file system, capacity, free space, and item count.
4. Browse directories in the left panel.
5. Select an item to view its details in the right panel.

If the image format or file system is unsupported, use **Visualization** to inspect the raw structure instead.

## Using the tools

The **Tools** tab groups Greaseweazle maintenance operations.

![Tools tab](images/main-tools-en.png)

Select a command from the list on the left, review its parameters, then click **Execute**. Destructive or hardware-changing commands should only be used after verifying the selected controller and drive.

The individual diagnostic dialogs are described in [Hardware diagnostics and maintenance](#hardware-diagnostics-and-maintenance).

## Emulation

### Opening a saved machine

The **Emulation** tab lists saved configurations. Select one and click **Open**. Each running machine appears in its own tab.

![Emulation welcome screen](images/main-emulation-welcome-en.png)

Create and edit machines in **Options > Emulation > Configurations** and **Options > Emulation > Amiga**.

### Running-machine controls

![Running emulated machine](images/main-emulation-running-en.png)

The running-machine toolbar provides power, pause, reset, save-state, load-state, capture, and display controls. It also shows:

- the configured quick-save and quick-load shortcuts;
- the active renderer, such as Direct3D 11;
- the fullscreen and mouse-release shortcuts;
- audio, controller, and mouse state;
- the current resolution, refresh rate, and frame rate.

The disk strip at the bottom of the emulation display manages removable media for each emulated drive. Keyboard assignments can be changed in **Options > Emulation > Shortcuts**, while emulated keyboard, mouse, and controller mappings are configured in the corresponding Amiga tabs.

## Application options

Open **Options** from the main window to configure the application.

### General

![General options](images/options-general-en.png)

The **General** tab contains:

- the default disk-image folder;
- interface language and theme;
- filename-tag generation for conversions;
- predefined and recent custom tag patterns;
- a live filename example.

Tag variables include the source name, family, format, extension, date, and time. Use the reset button to restore the default pattern.

### Logs

![Log options](images/options-logs-en.png)

Logging can be configured independently for each operation. For every category, choose whether to save logs, set a maximum file size, and decide whether previous logs should be retained. A size of `0` means unlimited. **Open folder** opens the current log directory.

### Controllers and drives

![Controllers and drives](images/options-controllers-and-drives-en.png)

Use this tab to:

- scan for connected controllers;
- add and remove drive configurations;
- select drive size, density, and speed;
- save hardware settings;
- choose or automatically find `gw.exe`;
- check for and download Greaseweazle Host Tools updates;
- restore a previously configured executable path.

Saved hardware settings remain available when a drive is temporarily disconnected.

### Engines

![Engine selection](images/options-engines-en.png)

Choose the engine independently for reading, writing, conversion, and Disk Explorer. The selected engine is used strictly: if it cannot perform the requested operation, GW GUI reports the limitation instead of silently switching engines.

### Profiles

![Profiles](images/options-profiles-en.png)

Profiles store reusable settings for read, write, and conversion operations. Select the relevant category to manage its profiles. A selected profile is shown in the main-window status bar and in operation screens.

## Emulation options

The **Emulation** options contain general storage settings, global shortcuts, saved configurations, and machine-specific settings.

### General emulation folders

![General emulation options](images/options-emulation-general-en.png)

Set the shared emulation storage folder and the default folders for captures and saved states. **Open folder** opens the shared location in File Explorer.

### Global shortcuts

![Emulation shortcuts](images/options-emulation-shortcuts-en.png)

Search for an action or key assignment, assign or remove shortcuts, restore defaults, and clear conflicts. The status column identifies valid and conflicting assignments.

### Saved configurations

![Saved emulation configurations](images/options-emulation-configurations-en.png)

This page lists saved machines. Select a configuration to edit it in the **Amiga** tab. You can refresh the list or delete the selected configuration.

## Amiga configuration

The current interface provides detailed Amiga configuration pages. The same settings structure can be extended for other emulated systems without changing the main workflow.

### General

![Amiga general settings](images/options-amiga-general-en.png)

Choose the Amiga model, save the configuration, install or replace the emulator version, and define default folders for hard disks and other media. **Search versions** queries the official emulator-version source.

### CPU

![Amiga CPU settings](images/options-amiga-cpu-en.png)

The CPU page shows the processor selected by the machine model and provides compatible precision, FPU, and speed choices. Options that do not apply to the selected model remain disabled.

### RAM

![Amiga RAM settings](images/options-amiga-ram-en.png)

Configure Chip RAM, Slow RAM, Fast RAM, and supported expansion memory. Compatibility messages explain restrictions for the selected machine, and the total configured memory is displayed at the bottom.

### ROM

![Amiga ROM settings](images/options-amiga-rom-en.png)

Select the system Kickstart ROM, optional extended ROM, and ROM key. The detected-ROM list displays names, revisions, and compatibility with the selected model. Select a detected ROM and click **Use**, or browse to a file manually.

ROM files are not supplied by GW GUI. Use ROMs you are legally permitted to use.

### Video

![Amiga video settings](images/options-amiga-video-en.png)

Configure video standard, aspect ratio, resolution, line mode, border cropping, renderer, color depth, frame skipping, gamma, and flicker fixing. Additional chipset settings are available further down the page when supported by the selected model.

### Audio

![Amiga audio settings](images/options-amiga-audio-en.png)

Enable or disable audio, choose the output device and latency, then configure interpolation, Amiga filtering, filter type, stereo separation, floppy-drive sound, and CD-audio volume.

### Storage

![Amiga storage settings](images/options-amiga-storage-en.png)

The storage page lists device identifiers, types, models, associated media, and available actions. Add, configure, or remove devices here. Floppy disks and CDs can be inserted or replaced directly from a running machine.

### Keyboard

![Amiga keyboard settings](images/options-amiga-keyboard-en.png)

Search Amiga keys and host assignments, assign new keys, remove mappings, restore defaults, or clear conflicts. The status column reports whether each assignment is valid.

### Mouse

![Amiga mouse settings](images/options-amiga-mouse-en.png)

Set physical mouse speed, choose which analog stick controls the mouse, adjust the analog dead zone and speed, and configure mouse-action mappings. Restore defaults or clear mapping conflicts when necessary.

### Controllers

![Amiga controller settings](images/options-amiga-controllers-en.png)

Detect connected controllers, assign devices and controller types to Amiga ports, and configure controller mappings and turbo-fire settings. Available choices depend on detected hardware and the selected machine.

## Hardware diagnostics and maintenance

These dialogs are opened from the **Tools** tab. Each dialog previews the generated Greaseweazle command. Review it before clicking **Execute**.

### Controller information

![Controller information](images/tool-controller-information-en.png)

Displays information reported by the selected controller. Expand **Raw output** when you need the complete command response.

### USB bandwidth

![USB bandwidth](images/tool-usb-bandwidth-en.png)

Measures the available USB communication bandwidth. Use it to diagnose unstable transfers or an unsuitable USB connection.

### Drive speed

![Drive speed](images/tool-drive-speed-en.png)

Measures the drive rotation speed. Increase the number of measurements when you need a more representative result.

### Seek head

![Seek head](images/tool-seek-head-en.png)

Moves the drive head to a selected cylinder. **Allow extreme cylinders** permits normally restricted positions, and **Keep motor active** leaves the motor running during the operation. Use extreme positions only when the hardware procedure explicitly requires them.

### Drive alignment diagnostic

![Drive alignment diagnostic](images/tool-drive-alignment-en.png)

Runs repeated reads for drive-alignment analysis. It supports track selection, revolution and read counts, decoding format, raw flux, index, speed, PLL, density-pin, hard-sector, TG43, and reverse-data options. Alignment work requires appropriate reference media and hardware knowledge.

### Hardware pins

![Hardware pins](images/tool-hardware-pins-en.png)

Reads or changes a supported controller pin. Select the pin, enable **Change pin** only when writing a value, and select **High level** when required by the intended hardware operation.

### Reset controller

![Reset controller](images/tool-reset-controller-en.png)

Resets the Greaseweazle controller. Use this when the controller is detected but no longer responds normally.

### Delays

![Controller delays](images/tool-delays-en.png)

Reads or changes controller timing values, including selection, head step, settle, motor, automatic deselection, write timing, and index mask delays. Enable only the values that you intend to modify.

### Firmware

![Firmware update](images/tool-firmware-en.png)

Updates controller firmware. **Update bootloader** is explicitly marked as risky and should remain disabled unless the official firmware procedure requires it. Do not disconnect the controller during an update.

## Logs and operation history

Open the operation history to inspect saved logs by operation.

![Operation history](images/operation-history-en.png)

Select a log on the left to display its contents. **Export** saves a copy for diagnostics or support. Paths and command lines may contain personal folder names, so review exported logs before sharing them.

The live console in the main window shows the current command and recent output. Its copy button copies the displayed text.

## Application data and portable use

GW GUI keeps user data separate from application binaries. Depending on the selected package and mode, settings, logs, downloaded tools, emulator components, captures, states, and machine configurations are stored either in the application `Data` directory or in the configured user-data locations.

Before replacing or moving a portable installation, keep the complete application folder together and back up the `Data` folder. Do not move individual files from `lib`, because the application resolves its own and third-party libraries from that structure.

## Troubleshooting

### The controller is not listed

1. Reconnect the controller directly to the computer.
2. Open **Options > Controllers and drives**.
3. Click **Scan**.
4. Verify the controller status and drive configuration.
5. Run **Controller information** if detection succeeds but commands fail.

### `gw.exe` cannot be found

Open **Options > Controllers and drives**, then use **Find gw.exe**, **Choose**, or **Download latest version**. Confirm that the detected path points to the intended Greaseweazle installation.

### An operation uses the wrong engine

Open **Options > Engines** and check the engine assigned to that exact operation. GW GUI does not silently fall back to the other engine.

### An image is not recognized

Disable automatic detection only if you know the correct machine and format. Otherwise, try the **Visualization** tab to inspect the image at a lower level.

### Emulation does not start

Verify the saved configuration, installed emulator version, selected ROM, storage paths, and model compatibility. Review the application log for the complete error details.

### A shortcut or input does not work

Check both the global **Emulation > Shortcuts** page and the machine-specific keyboard, mouse, or controller page. Resolve any assignment marked as conflicting.

### A command fails unexpectedly

1. Read the live console output.
2. Open **Operation history** for the complete saved log.
3. Confirm the selected controller, drive, profile, engine, and file paths.
4. Export the relevant log if it must be shared for diagnosis.
