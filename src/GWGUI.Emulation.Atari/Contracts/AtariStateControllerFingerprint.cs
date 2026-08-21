namespace GWGUI.Emulation.Atari;

internal sealed record AtariStateControllerFingerprint(
    int Port,
    AtariPeripheralCategory Peripheral,
    string? DeviceId,
    int DeadZonePercent,
    IReadOnlyList<KeyValuePair<string, string>> Mappings);
