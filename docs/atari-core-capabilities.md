# Atari Libretro cores — verified capability matrix

This document is the evidence record for `ATA-002` in `emulation-atari.md`. It describes the six source revisions locked by `ATA-001`. Source code takes precedence over the Libretro web pages and `.info` files when those documents lag behind the inspected revision.

Inspection date: 16 August 2026.

## Machines and content loading

| Core | Machines exposed by the core | Extensions reported by `retro_get_system_info` | `need_fullpath` | `block_extract` | No-content boot |
|---|---|---|---:|---:|---:|
| Hatari | `ST`, `STE`, `TT`, `Falcon` | `ST`, `MSA`, `ZIP`, `STX`, `DIM`, `IPF`, `VHD`, `GEM`, `IDE`, `M3U` | yes | no | no |
| Atari800 | `400/800 (OS B)`, `800XL (64K)`, `130XE (128K)`, `Modern XL/XE (320K Compy Shop)`, `Modern XL/XE (576K)`, `Modern XL/XE (1088K)`, `XEGS`, `5200` | `xfd`, `atr`, `dcm`, `cas`, `bin`, `a52`, `zip`, `atx`, `car`, `rom`, `com`, `xex`, `m3u` | yes | no | yes |
| Stella 2023 | Atari 2600 | `a26`, `bin` | no | no | no |
| ProSystem | Atari 7800 | `a78`, `bin`, `cdf` | no | no (zero-initialized) | no |
| Beetle Lynx | Atari Lynx | `lnx`, `lyx`, `bll`, `o` | no | no | no |
| Virtual Jaguar | Jaguar cartridges and Jaguar CD content | `j64`, `jag`, `rom`, `abs`, `cof`, `bin`, `prg`, `cue`, `cdi` | no | no (zero-initialized) | no |

Important consequences for GW GUI:

- Hatari exposes four machine values. `STF`, `STFM`, `Mega ST` and `Mega STE` are not distinct `hatari_machinetype` values in this revision. GW GUI may present historically meaningful presets, but it must map them explicitly to `ST` or `STE` and must not claim that Hatari exposes separate machine values.
- Atari800 exposes the eight values listed above. Generic `XL` and `XE` labels in GW GUI must resolve to one of those concrete memory configurations.
- Virtual Jaguar source at the inspected revision reports `cue` and `cdi` in addition to the cartridge extensions. The current `.info` file omits both, so the source is authoritative for Jaguar CD integration.
- None of the six cores registers a Libretro subsystem. Their `retro_load_game_special` implementations return `false`.

## Firmware and non-volatile data

| Core | Firmware contract | Required? | Persistent memory observed |
|---|---|---:|---|
| Hatari | `tos.img`; selectable TOS files may also be placed below `system/hatari/tos` | yes | Hatari configuration/files managed outside Libretro save RAM |
| Atari800 | Optional original ROMs: `5200.rom`, `ATARIBAS.ROM`, `ATARIOSA.ROM`, `ATARIOSB.ROM`, `ATARIXL.ROM`, `BB01R4_OS.ROM`, `XEGAME.ROM`; bundled AltirraOS replacements permit operation without the originals | no for the core as shipped | emulator configuration and media writes; exact Libretro memory exposure remains recorded below |
| Stella 2023 | no external firmware | no | no `SAVE_RAM` block exposed by this core revision |
| ProSystem | optional `7800 BIOS (U).rom`; the source also recognizes the PAL BIOS path used by the emulator | no | no `SAVE_RAM` block exposed by this core revision |
| Beetle Lynx | `lynxboot.img` | yes | no `SAVE_RAM` block exposed by this core revision |
| Virtual Jaguar | console boot ROM and both Jaguar CD BIOS images are embedded; optional CD overrides include `[BIOS] Atari Jaguar CD (World).j64` and `[BIOS] Atari Jaguar Developer CD (World).j64` | no | one frontend `.srm` containing cartridge EEPROM, CD EEPROM and, for CD content, Memory Track NVRAM |

The firmware catalogue in GW GUI must distinguish “optional original ROM because an open replacement is embedded” from “required firmware”. In particular, the optional flags in `atari800_libretro.info` are intentional even though older web documentation calls several original ROMs required.

## Disk Control and multi-media behavior

| Core | Standard Disk Control | Extended Disk Control | Multi-disc/list behavior | Other removable media |
|---|---:|---:|---|---|
| Hatari | yes | yes when frontend interface version permits | `M3U`; disk insertion/ejection/index through the registered callbacks | VHD, IDE and GEMDOS marker content are loaded as primary content, not through a Libretro subsystem |
| Atari800 | yes | yes when frontend interface version permits | `M3U`; disk insertion/ejection/index through the registered callbacks | cassette and cartridge are content types, not Libretro Disk Control drives |
| Stella 2023 | no | no | none | cartridge content only |
| ProSystem | no | no | none | cartridge content only |
| Beetle Lynx | no | no | none | cartridge content only |
| Virtual Jaguar | no | no | no Libretro disc changer | Jaguar CD is loaded as `.cue` or `.cdi` primary content; cartridge and CD are mutually determined by content |

This corrects older Libretro feature tables that still mark Hatari and Atari800 Disk Control as unsupported: both inspected sources register the standard callback and prefer the extended callback when the frontend reports a sufficient interface version.

## Save states

All six inspected cores implement `retro_serialize_size`, `retro_serialize` and `retro_unserialize`.

| Core | Verified constraints from source |
|---|---|
| Hatari | advertises a fixed 10 MiB capacity (the source comments that an uncompressed state is about 6 MiB); requires room for its version byte and mapper header, then serializes mapper state plus Hatari’s memory snapshot |
| Atari800 | allocates at most 1,300,000 bytes to calculate the actual state length; save/load delegate the supplied length to the Atari state routines and report their success |
| Stella 2023 | normally reports Stella’s dynamic state size; when the frontend requests run-ahead serialization it advertises the 1 MiB maximum; save/load delegate size validation to Stella |
| ProSystem | exactly 49,221 bytes normally or exactly 83,968 bytes for frontend-requested fast states; any other supplied size is rejected |
| Beetle Lynx | calculates the current Mednafen state length first; save uses a buffer of the frontend-supplied capacity and load passes the supplied length to Mednafen |
| Virtual Jaguar | writes 2,621,440-byte version-9 states; validates magic/version and accepts older supported layouts down to the 2,490,368-byte version-8 floor |

State support does not prove cross-version compatibility. GW GUI must retain the core identity/version metadata beside every state and report an explicit incompatibility instead of silently loading with another core build.

## Core-option inventory

The authoritative English definitions are:

- Hatari: `libretro/libretro_core_options.h` — 50 distinct `hatari_*` keys in the inspected source.
- Atari800: `libretro/libretro_core_options.h` — 30 distinct `atari800_*` keys.
- Stella 2023: `src/os/libretro/libretro.cxx` — 16 legacy `retro_variable` keys.
- ProSystem: `core/libretro_core_options.h` — 4 distinct `prosystem_*` keys.
- Beetle Lynx: `libretro_core_options.h` — 3 distinct `lynx_*` keys.
- Virtual Jaguar: `libretro_core_options.h` — 74 distinct `virtualjaguar_*` keys, including per-port mappings.

The complete inventory of the 177 distinct keys, their accepted values, declared defaults and dynamic visibility rules is recorded in [`atari-core-options.md`](atari-core-options.md). The inventory also preserves the invalid `atari800_ntscpal=disabled` default found in the source instead of silently rewriting it.

## Input ports and identifiers

“Registered” below means that the core sends `RETRO_ENVIRONMENT_SET_CONTROLLER_INFO`. “Polled” means that its current source actually invokes the input callback for that device or identifier. GW GUI must satisfy the polled contract even when the core does not publish a controller description.

| Core | Ports/devices registered | Devices and identifiers actually polled |
|---|---|---|
| Hatari | Descriptions exist for ports 1 and 2 (`Atari Joystick`, `Atari Keyboard`), but registration is commented out in this revision | ports 1–2 joypad directions/buttons; both analog-stick X/Y axes; keyboard key IDs 0–319; relative mouse X/Y and left/right buttons; pointer X/Y/pressed for touchscreen-style pointing |
| Atari800 | four ports; each offers `Atari Joystick`, `Atari 5200 Joystick` and `Atari Keyboard` | ports 1–4 joypad; analog axes for 5200 controls; keyboard; relative mouse buttons and axes |
| Stella 2023 | four ports; `Automatic` (`RETRO_DEVICE_JOYPAD`) or `None` | joypad directions and buttons, analog paddle axes, relative mouse and light-gun coordinates/trigger according to the selected emulated controller |
| ProSystem | no controller-info table registered | two joypads (including bitmask reads when supported) and analog axes used for controller input |
| Beetle Lynx | no controller-info table registered | port 1 joypad directions and buttons |
| Virtual Jaguar | no controller-info table registered | two joypad bitmasks; left/right analog-stick X/Y axes; keyboard keys used as Jaguar numeric keypads |

The frontend-facing Atari configuration must therefore describe the real machine ports itself. It must not infer absence of mouse, keyboard, analog or light-gun input merely because a core omitted `SET_CONTROLLER_INFO`.

## Libretro memory exposure

This table concerns `retro_get_memory_data`/`retro_get_memory_size`, not save-state contents.

| Core | Exposed memory |
|---|---|
| Hatari | none; all IDs return null/zero |
| Atari800 | `SYSTEM_RAM`, first 64 KiB only, even for machine options with more configured RAM |
| Stella 2023 | `SYSTEM_RAM`, pointer and size supplied dynamically by Stella |
| ProSystem | `SYSTEM_RAM`, 64 KiB |
| Beetle Lynx | `SYSTEM_RAM`, 64 KiB |
| Virtual Jaguar | `SYSTEM_RAM`, 2 MiB; `SAVE_RAM`, 128 bytes for a normal cartridge, 128 KiB for the Memory Track cartridge, or 131,328 bytes for CD mode (cartridge EEPROM + CD EEPROM + 128 KiB Memory Track) |

Only Virtual Jaguar exposes persistent `SAVE_RAM` through this API in the inspected revisions. Cartridge-dependent persistence must not be advertised for Stella, ProSystem or Beetle Lynx unless a later verified core revision actually exposes it.

## Video, region and audio contract

| Core | Pixel format | Geometry | Region / video rate | Audio rate |
|---|---|---|---|---:|
| Hatari | RGB565 | base changes with machine/resolution; observed declared families include 392×248, 416×274, 366×243, 640×480, 732×486 and 832×548; maximum 832×548; 4:3 | runtime cadence follows PAL/NTSC options (initially 50 Hz), but `retro_get_region` always returns NTSC | 44,100 Hz |
| Atari800 | XRGB8888 (RGB565 in the 16-bit build) | base resolution is option-dependent; maximum 400×300; 4:3 | cadence follows PAL/NTSC selection, but `retro_get_region` always returns NTSC | 44,100 Hz |
| Stella 2023 | XRGB8888 | base width/height depend on the cartridge signal; maximum width is the NTSC-filter output width for 160 source pixels and maximum height is 312 | NTSC 60 Hz or PAL 50 Hz; region is reported dynamically | 31,440 Hz NTSC; 31,200 Hz PAL |
| ProSystem | XRGB8888, with RGB565 fallback | dynamic width; 223 lines NTSC or 272 PAL; maximum 320×292; 4:3 | NTSC 60 Hz or PAL 50 Hz; region is reported dynamically | 31,440 Hz NTSC; 31,200 Hz PAL |
| Beetle Lynx | RGB565 by default; XRGB8888 option; 0RGB1555 compatibility fallback when required | fixed 160×102; aspect 80:51 | 75 Hz by default or forced 60 Hz; `retro_get_region` returns NTSC | 44,100 Hz |
| Virtual Jaguar | XRGB8888 | dynamic base; maximum 652×256 at 1× internal resolution or 1304×512 at 2×; 4:3 | NTSC 60 Hz or PAL 50 Hz; region is reported dynamically | 48,000 Hz |

GW GUI must accept geometry and timing changes after content load. It must not hard-code the value returned by `retro_get_region` as the video cadence for Hatari, Atari800 or Beetle Lynx.

## Verified limitations and integration boundaries

- Hatari has only the four core machine values `ST`, `STE`, `TT` and `Falcon`; more precise labels are GW GUI presets, not extra Hatari machines.
- Atari800 exposes only the first 64 KiB through `SYSTEM_RAM`, regardless of its larger XL/XE memory options.
- The declared Atari800 default `atari800_ntscpal=disabled` is not one of its accepted values; GW GUI must select a valid explicit value and must not offer `disabled`.
- Hatari and Atari800 can change timing independently of the region value they report. The frontend must use the AV timing callback as authoritative.
- Hatari controller descriptions are present in source but not registered; ProSystem, Beetle Lynx and Virtual Jaguar also omit a controller-info table. Their actual callback reads remain required.
- Stella, ProSystem and Beetle Lynx do not expose Libretro `SAVE_RAM` in these revisions. Save-state support is a different mechanism and must not be presented as cartridge persistence.
- Virtual Jaguar CD content is primary `.cue`/`.cdi` content. It has no Libretro disc changer and cannot be treated like a multi-disc Disk Control drive.
- None of the six cores exposes a Libretro subsystem, so no standalone-emulator feature may be mapped to `retro_load_game_special` without new source evidence.
- Save states are core-build-specific. No cross-core or cross-version compatibility is guaranteed by the Libretro API.
- Core options hidden dynamically by Virtual Jaguar must remain stored without being applied to the wrong media type; the other five inspected cores publish no dynamic visibility callback.

## Source evidence

- Core metadata: the six `retro_get_system_info` implementations and the matching files from [`libretro-core-info`](https://github.com/libretro/libretro-core-info).
- Machine choices and option values: the English option definitions in each inspected core, not translated option headers.
- Disk Control: Hatari `libretro/libretro.c` and Atari800 `libretro/libretro-core.c`, which register standard and extended callback structures.
- Save states: each core’s exported `retro_serialize_size`, `retro_serialize` and `retro_unserialize` implementation.
- Virtual Jaguar CD behavior and firmware: the inspected source and the current official [Virtual Jaguar documentation](https://docs.libretro.com/library/virtual_jaguar/).
