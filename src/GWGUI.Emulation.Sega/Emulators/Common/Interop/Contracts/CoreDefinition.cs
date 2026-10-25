namespace GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

internal sealed record CoreDefinition(string Id, string LibraryName, string LibraryFileName,
    IReadOnlyList<string> Extensions, CoreReleaseSettings ReleaseSettings,
    IReadOnlyList<CoreOption> Options, IReadOnlyList<FirmwareSlot> Firmware,
    EmulationEmulatorDefinition Emulator,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>? MachineOptions = null,
    Action<MachineConfiguration, string>? PrepareSystem = null,
    Func<MachineConfiguration, string, string, string>? PrepareContent = null,
    bool SelectFirstInsertedMedia = false);
