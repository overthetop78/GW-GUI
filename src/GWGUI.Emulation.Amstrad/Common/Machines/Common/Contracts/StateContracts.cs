namespace GWGUI.Emulation.Amstrad.Common.Machines.Common.Contracts;

internal sealed record SavedStateHeader(
    int FormatVersion,
    string Model,
    string CoreSha256,
    IReadOnlyDictionary<string, string>? Options,
    IReadOnlyList<string> MediaSha256s,
    string StateSha256);
