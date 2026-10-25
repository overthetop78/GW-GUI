# GWGUI.Emulation.SDK

`GWGUI.Emulation.SDK` provides the public contracts used to develop independent emulation modules
for GW GUI. A module can define machines, settings, runtimes, media, input, video, audio, saved
states and its own update source without referencing `GWGUI.App`.

## Install

```xml
<PackageReference Include="GWGUI.Emulation.SDK" Version="1.2.0" />
```

The package targets .NET 10. The current host API is `1.2`, and the current `module.json` schema is
`2`.

## Start a module

1. Copy the [`sdk/module-template`](https://github.com/overthetop78/GW-GUI/tree/main/sdk/module-template)
   directory into a new repository.
2. Replace the example project identity, namespace, assembly name and values in `module.json`.
3. Implement `IEmulationModuleFactory` and return an `IEmulationModule` from `Create`.
4. Require host API `1.2` when using the current `AudioChunk` constructor or `IEmulationVideoSettingsManager`. The workspace modules and template target the current development API directly; declare the tested maximum explicitly.
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
- `IEmulationVideoSettingsManager` applies the host's `EmulationGraphicsApi` preference to an opaque configuration without exposing native backend values to the application.
- `AudioChunk.InterleavedSamples` carries PCM16 samples in ascending `EmulationAudioChannelLayout` speaker order. Its `ChannelCount` is derived from that layout, and `HasValidLength` checks the complete interleaved frames. The default layout is stereo.
- `IMultichannelAudioOutput` adds `Start(sampleRate, channelLayout)` to the audio output contract. A runtime must preserve the layout through buffering, transport, volume changes and output replacement, and reconfigure the output when either the sample rate or layout changes. A stereo-only `IAudioOutput` must not receive surround samples as stereo.
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

## Shared video preference (host API 1.2)

Implement `IEmulationVideoSettingsManager.ApplyVideoSettings` to follow the application's existing presentation renderer. Return a configuration derived without mutating the input. The module chooses the supported native API and its fallback; native tokens stay inside its adapter. Preserve software rendering and special emulation modes as separate choices. `Automatic` also covers a host presentation path with no specific graphics API, such as WPF; it does not force software emulation.

GW GUI applies the preference while editing and saving settings and before runtime creation. Modules that omit this optional interface keep their existing behavior. Modules implementing it must declare host API minimum 1.2 so older hosts reject them before loading their assembly.
