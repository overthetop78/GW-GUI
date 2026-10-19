namespace GWGUI.Emulation.Wenquxing.Common.Machines.Common.Contracts;



internal sealed record StateConfigurationFingerprint( int SchemaVersion, MachineModel Model, Emulator Core, bool AudioEnabled, IReadOnlyList<StateContentEntry> Content, IReadOnlyList<KeyValuePair<string, string>> Options, StateInputFingerprint Input, IReadOnlyList<MediaConfiguration> Media);






internal sealed record StateInputFingerprint(IReadOnlyList<KeyValuePair<string, string>> Keyboard);
