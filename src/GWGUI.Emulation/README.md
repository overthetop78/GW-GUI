# GWGUI.Emulation.SDK

`GWGUI.Emulation.SDK` provides the public contracts used to develop independent emulation modules
for GW GUI. A module can define machines, settings, runtimes, media, input, video, audio, saved
states and its own update source without referencing `GWGUI.App`.

## Install

```xml
<PackageReference Include="GWGUI.Emulation.SDK" Version="1.1.0" />
```

The package targets .NET 10. The current host API is `1.1`, and the current `module.json` schema is
`2`.

## Start a module

1. Copy the [`sdk/module-template`](https://github.com/overthetop78/GW-GUI/tree/main/sdk/module-template)
   directory into a new repository.
2. Replace the example project identity, namespace, assembly name and values in `module.json`.
3. Implement `IEmulationModuleFactory` and return an `IEmulationModule` from `Create`.
4. Set `hostApiMinimum` and `hostApiMaximum` to `1.1` when using the optional controller feedback API. Existing modules using only API 1.0 may retain minimum `1.0` after checking compatibility with host API 1.1.
5. Set `updateCatalogUrl` to the stable HTTPS JSON catalogue published by the module repository.
6. Package the result with this structure:

```text
Modules/<module-id>/
  module.json
  gwgui.emulation.<module-id>.dll
  <private dependencies>
```

GW GUI validates the manifest, API range, identity, archive paths and checksum before installation.
Adding, updating or removing a module takes effect after restarting GW GUI.

## Main contracts

- `IEmulationModuleFactory` is the module entry point.
- `IEmulationModule` describes machines, settings, configurations and runtime creation.
- `IEmulatedMachine` exposes lifecycle, input, media, video, audio and saved-state services.
- `IEmulationEmulatorManager`, `IEmulationFirmwareManager`, `IEmulationInputSettingsManager` and
  `IEmulationStorageSettingsManager` add optional capabilities.
- `IEmulationModuleLocalization` supplies translations embedded in the module. The application owns
  the localization display and lookup flow, while a module owns only the translated strings specific
  to its machines and emulators. This keeps the application independent from concrete modules: it
  requests a key through the interface without referencing Atari, Amiga, Amstrad or any emulator.

The complete contract, manifest rules, packaging format and publication workflow are documented in
the [module authoring guide](https://github.com/overthetop78/GW-GUI/blob/main/docs/architecture/emulation-module-authoring.md).
SDK compatibility and version changes are documented in the
[versioning policy](https://github.com/overthetop78/GW-GUI/blob/main/docs/architecture/emulation-sdk-versioning.md).

## Version compatibility

The NuGet package follows semantic versioning. Every module declares an inclusive host API range in
`module.json`; compatibility beyond that range is never assumed. A package revision can improve
documentation or internal implementation while retaining host API `1.0`.

## License

MIT


## Optional controller feedback (host API 1.1)

A machine may implement `IEmulationControllerFeedbackSource`. Its complete snapshot contains only explicitly configured physical device/motor destinations and normalized strengths. An empty snapshot clears previous outputs. The host must also stop its outputs when inactive, paused, stopped, faulted or disposed, and must attempt every device even if one fails. It never takes ownership of a borrowed physical input service.

`EmulationControllerChoice.VisualOutputDefinitions` describes the commands supported by each visual peripheral; `EmulationControllerPort.OutputBindings` contains the user's explicit destinations. Missing bindings remain empty, including after restoring defaults. Core-specific effect codes belong to the backend adapter, not these contracts. A module using these types must require host API 1.1; an older host must reject its manifest before loading its assembly.
