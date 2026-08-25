namespace GWGUI.Emulation.Amiga.Contracts;

public sealed record AmigaFirmware(
    string Path,
    long Size,
    string Md5,
    string Sha256,
    DateTime LastWriteTimeUtc,
    AmigaFirmwareType Type,
    bool IsKnown,
    bool IsOfficial,
    string? Name,
    string? Version,
    IReadOnlyList<string> CompatibleModels);
