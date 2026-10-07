namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Contracts;

internal sealed record SavedStateHeader(
    int FormatVersion,
    string Model,
    string CoreSha256,
    IReadOnlyDictionary<string, string> FirmwareSha256s,
    IReadOnlyDictionary<string, string> Options,
    string StateSha256,
    IReadOnlyList<string> MediaSha256s);
