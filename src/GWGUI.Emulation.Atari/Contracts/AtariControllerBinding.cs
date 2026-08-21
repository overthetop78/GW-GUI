namespace GWGUI.Emulation.Atari;

public sealed record AtariControllerBinding(
    int Port,
    AtariPeripheralCategory Peripheral,
    string? DeviceId = null,
    IReadOnlyDictionary<string, string>? Mappings = null,
    int DeadZonePercent = AtariControllerConstants.DefaultDeadZonePercent);
