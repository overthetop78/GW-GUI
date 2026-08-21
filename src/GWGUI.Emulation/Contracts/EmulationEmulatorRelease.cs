namespace GWGUI.Emulation;

public sealed record EmulationEmulatorRelease(
    string Id,
    string DisplayName,
    string Version,
    bool IsRequired = false);
