namespace GWGUI.Emulation.Atari;

internal sealed record AtariSavedStateHeader(
    int FormatVersion,
    AtariCoreKind Core,
    string CoreName,
    string CoreVersion,
    string CoreSha256,
    AtariMachineModel Model,
    string ConfigurationSha256,
    string ContentSha256,
    string StateSha256);

internal sealed record AtariStateFile(AtariSavedStateHeader Header, byte[] State);

internal sealed record AtariStateContentEntry(string Category, string Kind, string Hash);

internal sealed record AtariStateConfigurationFingerprint(
    int SchemaVersion,
    AtariMachineModel Model,
    AtariCoreKind Core,
    bool AudioEnabled,
    GWGUI.Emulation.EmulationVideoRenderer VideoRenderer,
    IReadOnlyList<AtariStateContentEntry> Content,
    IReadOnlyList<KeyValuePair<string, string>> Options,
    AtariStateInputFingerprint Input,
    IReadOnlyList<AtariMediaConfiguration> Media,
    IReadOnlyList<AtariFirmwareConfiguration> Firmwares);

internal sealed record AtariStateInputFingerprint(
    IReadOnlyList<KeyValuePair<string, string>> KeyboardMappings,
    IReadOnlyList<AtariStateControllerFingerprint> Controllers,
    string? MouseDeviceId,
    bool CaptureMouse,
    string ReleaseMouseKey);

internal sealed record AtariStateControllerFingerprint(
    int Port,
    AtariPeripheralKind Peripheral,
    string? DeviceId,
    int DeadZonePercent,
    IReadOnlyList<KeyValuePair<string, string>> Mappings);
