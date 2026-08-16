# Atari external-core contract

This document records the comparison required by ATA-004 between the existing Amiga external-core ABI and the six Atari engines selected by the project. It is an implementation boundary: only byte-for-byte identical native contracts belong to `GWGUI.Emulation/Common`; machine behavior remains in its specialized project.

## Shared exported lifecycle

All six Atari engines and the Amiga engine expose the same functions used by GW GUI for:

- API-version and system-information discovery;
- environment, video, audio and input callback registration;
- initialization, deinitialization, content loading and unloading;
- controller-port selection, reset and frame execution;
- runtime video/timing discovery;
- state-size discovery, serialization and restoration;
- system-memory and persistent-memory discovery.

The native delegates for these exports are therefore defined once in `ExternalCoreApi`. Export names remain private implementation strings inside each specialized loader; they are not exposed by the public GW GUI contracts.

## Shared structures and callbacks

| Contract in `ExternalCoreApi` | Amiga | Six Atari engines | Classification |
|---|---:|---:|---|
| `SystemInfo`, `GameInfo` | yes | yes | common byte layout |
| `Geometry`, `Timing`, `SystemAvInfo` | yes | yes | common byte layout; values are engine-specific |
| environment callback | yes | yes | common signature; command handling stays specialized |
| video callback | yes | yes | common signature; accepted formats and geometry remain specialized |
| sample and batch audio callbacks | yes | yes | common signatures; rates remain specialized |
| input polling and state callbacks | yes | yes | common signatures; ports and devices remain specialized |
| `Variable`, option-display structures | yes | yes | common byte layout; keys, values and visibility rules remain specialized |
| standard and extended disk-control structures | yes | Hatari and Atari800 only | common optional ABI; media policy remains specialized |
| keyboard callback | yes | used according to engine | common optional ABI; key mapping remains specialized |
| controller-description structures | yes | optionally registered | common optional ABI; port catalogue remains specialized |
| message and log structures | yes | optionally used | common optional ABI |
| LED callback structure | yes | optionally used | common optional ABI; LED meaning remains specialized |

The command identifiers used by the environment callback are stored separately in `ExternalCoreApiConstants`. Their numeric values and experimental flag are ABI data, not Amiga or Atari configuration.

## Shared managed infrastructure

The following implementation is identical for both machine families and lives in `GWGUI.Emulation/Common`:

- native-library loading, export resolution and deterministic unloading;
- temporary UTF-8 native strings with deterministic release;
- host-process strings and bounded binary blobs;
- input, video-frame, shared-video, audio and LED serialization;
- named limits for payloads, shared-video slots, input counts, audio chunks and LED states.

The specialized Amiga host remains a thin wrapper over these protocol functions so its existing command names and error identity remain unchanged. The Atari host will use the same functions with its own host name and its own command enum.

## Elements that must remain specialized

The following are deliberately excluded from `Common`:

- machine and model catalogues;
- firmware names, requirements and validation;
- content extensions and media-slot rules;
- disk, cartridge, cassette and CD behavior;
- option keys, defaults, accepted values and dynamic visibility;
- keyboard, mouse, light-gun, keypad and controller mappings;
- video modes, cadence, audio rates and renderer restrictions;
- save-memory policy and state compatibility metadata;
- user-visible diagnostics and translations.

For Atari these elements must be implemented under `GWGUI.Emulation.Atari` and use Atari names. They may only move to `Common` later if the Amiga implementation consumes the exact same contract without a machine-specific branch.

## Verification

`ExternalCoreApiTests` verifies sequential native layouts, pointer offsets, callback calling conventions, environment-command flags and deterministic native-library unloading. `EmulationHostProtocolFunctionsTests` verifies common input, video, audio, binary-payload and UTF-8 behavior. The complete Amiga-targeted suite is also required to pass before ATA-004 can be checked.
