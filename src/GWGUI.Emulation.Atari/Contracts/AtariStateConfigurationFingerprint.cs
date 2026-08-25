namespace GWGUI.Emulation.Atari.Contracts;

internal sealed record AtariStateConfigurationFingerprint(
    int SchemaVersion,
    AtariMachineModel Model,
    AtariEmulator Core,
    bool AudioEnabled,
    GWGUI.Emulation.Enums.EmulationVideoRenderer VideoRenderer,
    IReadOnlyList<AtariStateContentEntry> Content,
    IReadOnlyList<KeyValuePair<string, string>> Options,
    AtariStateInputFingerprint Input,
    IReadOnlyList<AtariMediaConfiguration> Media,
    IReadOnlyList<AtariFirmwareConfiguration> Firmwares);
