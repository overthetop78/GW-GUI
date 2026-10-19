namespace GWGUI.Emulation.Contracts;

internal sealed record EmulationLegacySavedStateHeader(
    int FormatVersion,
    string Model,
    string CoreSha256,
    IReadOnlyDictionary<string, string>? Options,
    IReadOnlyList<string> MediaSha256s,
    string StateSha256);
