namespace GWGUI.Emulation.Amiga.Common.Contracts;

public sealed record Firmware(
    string Path,
    long Size,
    string Md5,
    string Sha256,
    DateTime LastWriteTimeUtc,
    FirmwareType Type,
    bool IsKnown,
    bool IsOfficial,
    string? Name,
    string? Version,
    IReadOnlyList<string> CompatibleModels);
