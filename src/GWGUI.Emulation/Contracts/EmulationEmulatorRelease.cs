namespace GWGUI.Emulation.Contracts;

public sealed record EmulationEmulatorRelease(
    string Id,
    string DisplayName,
    string Version,
    bool IsRequired = false);
