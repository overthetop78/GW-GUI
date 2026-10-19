namespace GWGUI.Emulation.Epoch.Common.Machines.Common.Contracts;



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







internal sealed record StateInputFingerprint(IReadOnlyList<KeyValuePair<string, string>> Keyboard, IReadOnlyList<StateControllerFingerprint> Controllers);

