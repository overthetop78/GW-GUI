namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationStateControllerFingerprint<TPeripheral>(
    int Port,
    TPeripheral Peripheral,
    string? DeviceId,
    int DeadZonePercent,
    IReadOnlyList<KeyValuePair<string, string>> Mappings);
