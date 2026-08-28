namespace GWGUI.Emulation.Contracts;

public sealed record EmulationFirmwareCandidate(
    string Id,
    string Path,
    string DisplayName,
    string? Version,
    EmulationFirmwareCompatibility Compatibility,
    string? DestinationFieldId = null);
