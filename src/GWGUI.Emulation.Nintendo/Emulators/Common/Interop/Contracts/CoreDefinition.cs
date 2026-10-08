namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

internal sealed record CoreDefinition(string Id, string LibraryName, string LibraryFileName,
    IReadOnlyList<string> Extensions, CoreReleaseSettings ReleaseSettings,
    IReadOnlyList<CoreOption> Options, IReadOnlyList<FirmwareSlot> Firmware,
    EmulationEmulatorDefinition Emulator);
