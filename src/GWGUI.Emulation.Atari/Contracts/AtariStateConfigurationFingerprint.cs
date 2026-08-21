namespace GWGUI.Emulation.Atari;

internal sealed record AtariStateConfigurationFingerprint(
    int SchemaVersion,
    AtariMachineModel Model,
    AtariEmulator Core,
    bool AudioEnabled,
    GWGUI.Emulation.EmulationVideoRenderer VideoRenderer,
    IReadOnlyList<AtariStateContentEntry> Content,
    IReadOnlyList<KeyValuePair<string, string>> Options,
    AtariStateInputFingerprint Input,
    IReadOnlyList<AtariMediaConfiguration> Media,
    IReadOnlyList<AtariFirmwareConfiguration> Firmwares);
