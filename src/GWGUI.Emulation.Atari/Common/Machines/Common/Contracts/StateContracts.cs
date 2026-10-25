namespace GWGUI.Emulation.Atari.Common.Machines.Common.Contracts;



internal sealed record StateConfigurationFingerprint(
    int SchemaVersion,
    MachineModel Model,
    Emulator Core,
    bool AudioEnabled,
    IReadOnlyList<StateContentEntry> Content,
    IReadOnlyList<KeyValuePair<string, string>> Options,
    StateInputFingerprint Input,
    IReadOnlyList<MediaConfiguration> Media,
    IReadOnlyList<FirmwareConfiguration> Firmwares);







internal sealed record StateInputFingerprint(IReadOnlyList<KeyValuePair<string, string>> KeyboardMappings, IReadOnlyList<StateControllerFingerprint> Controllers, string? MouseDeviceId, bool CaptureMouse, string ReleaseMouseKey);

public sealed record StoredStateMetadata(
    string Name,
    StoredStateCategory Category,
    DateTimeOffset CreatedAtUtc,
    string StateFileName,
    string? CaptureFileName,
    Emulator Core,
    string CoreName,
    string CoreVersion,
    MachineModel Model,
    string ConfigurationSha256,
    string ContentSha256);
