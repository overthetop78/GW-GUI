namespace GWGUI.Emulation.Amiga.Common.Machines.Common.Contracts;

internal sealed record SavedStateHeader(int FormatVersion, string Model, string CoreSha256,
    string KickstartSha256, string? MediaSha256, IReadOnlyDictionary<string, string>? Options,
    string? ExtendedRomSha256 = null, string? RomKeySha256 = null, string? StateSha256 = null,
    IReadOnlyList<string>? MediaSha256s = null);
