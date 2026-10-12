namespace GWGUI.Emulation.Watara.Common.Machines.Common.Contracts;

internal sealed record SavedStateHeader( int FormatVersion, Emulator Core, string CoreName, string CoreVersion, string CoreSha256, MachineModel Model, string ConfigurationSha256, string ContentSha256, string StateSha256);

internal sealed record StateConfigurationFingerprint( int SchemaVersion, MachineModel Model, Emulator Core, bool AudioEnabled, IReadOnlyList<StateContentEntry> Content, IReadOnlyList<KeyValuePair<string, string>> Options, StateInputFingerprint Input, IReadOnlyList<MediaConfiguration> Media);

internal sealed record StateContentEntry(string Slot, string Category, string Hash);

internal sealed record StateControllerFingerprint( int Port, PeripheralCategory Peripheral, string? DeviceId, int DeadZonePercent, IReadOnlyList<KeyValuePair<string, string>> Mappings);

internal sealed record StateFile(SavedStateHeader Header, byte[] State);

internal sealed record StateInputFingerprint(IReadOnlyList<StateControllerFingerprint> Controllers);
