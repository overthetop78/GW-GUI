namespace GWGUI.Emulation;

public sealed record EmulationFirmwareCandidate(
    string Id,
    string Path,
    string DisplayName,
    string? Version,
    EmulationFirmwareCompatibility Compatibility);
