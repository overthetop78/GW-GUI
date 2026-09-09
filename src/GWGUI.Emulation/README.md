# GWGUI.Emulation.SDK

`GWGUI.Emulation.SDK` provides the public contracts used to develop independent emulation modules
for GW GUI. A module can define machines, settings, runtimes, media, input, video, audio, saved
states and its own update source without referencing `GWGUI.App`.

## Install

```xml
<PackageReference Include="GWGUI.Emulation.SDK" Version="1.0.1" />
```

The package targets .NET 10. The current host API is `1.0`, and the current `module.json` schema is
`2`.

## Start a module

1. Copy the [`sdk/module-template`](https://github.com/overthetop78/GW-GUI/tree/main/sdk/module-template)
   directory into a new repository.
2. Replace the example project identity, namespace, assembly name and values in `module.json`.
3. Implement `IEmulationModuleFactory` and return an `IEmulationModule` from `Create`.
4. Keep `hostApiMinimum` and `hostApiMaximum` at `1.0` while targeting this SDK API.
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
- `IEmulationModuleLocalization` supplies translations embedded in the module.

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
