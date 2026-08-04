namespace GWGUI.Domain.Formats;

public sealed record ImageExtension(string Extension, string DisplayName, bool IsDefault = false);

public sealed record DiskFormat(
    string Id,
    string Family,
    string DisplayName,
    IReadOnlyList<ImageExtension> Extensions,
    bool IsCommon = true,
    IReadOnlySet<string>? CompatibleSourceExtensions = null);

public interface IImageFormatCatalog
{
    IReadOnlyList<DiskFormat> Formats { get; }
    IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension);
}

public sealed class BuiltInImageFormatCatalog : IImageFormatCatalog
{
    public IReadOnlyList<DiskFormat> Formats { get; } =
    [
        new("raw.scp", "Raw", "Image brute", [new(".scp", "SuperCard Pro", true)]),
        new("amiga.amigados", "Amiga", "AmigaDOS — 880 Kio", [new(".adf", "Amiga Disk File", true)], CompatibleSourceExtensions: Set(".scp", ".adf", ".hfe")),
        new("amiga.amigadoshd", "Amiga", "AmigaDOS HD — 1,76 Mio", [new(".adf", "Amiga Disk File", true)], CompatibleSourceExtensions: Set(".scp", ".adf", ".hfe")),
        new("atarist.720", "Atari ST", "Atari ST — 720 Kio", [new(".st", "Image Atari ST", true), new(".msa", "Magic Shadow Archiver")], CompatibleSourceExtensions: Set(".scp", ".st", ".msa", ".hfe")),
        new("atarist.1440", "Atari ST", "Atari ST — 1,44 Mio", [new(".st", "Image Atari ST", true)], CompatibleSourceExtensions: Set(".scp", ".st", ".hfe")),
        new("ibm.360", "IBM PC", "IBM PC — 360 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe")),
        new("ibm.720", "IBM PC", "IBM PC — 720 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe")),
        new("ibm.1440", "IBM PC", "IBM PC — 1,44 Mio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe")),
        new("commodore.1541", "Commodore", "Commodore 64 — 1541", [new(".d64", "Image Commodore D64", true)], CompatibleSourceExtensions: Set(".scp", ".d64", ".hfe")),
        new("acorn.adfs.800", "Acorn", "Acorn ADFS — 800 Kio", [new(".adf", "Acorn Disk File", true)], CompatibleSourceExtensions: Set(".scp", ".adf", ".hfe")),
        new("raw.hfe", "Raw", "Image de flux HxC", [new(".hfe", "HxC Floppy Emulator", true)], CompatibleSourceExtensions: Set(".scp", ".hfe"), IsCommon: false)
    ];

    public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension)
    {
        var normalized = Normalize(sourceExtension);
        return Formats.Where(format => format.CompatibleSourceExtensions?.Contains(normalized) == true).ToArray();
    }

    private static HashSet<string> Set(params string[] values) => new(values.Select(Normalize), StringComparer.OrdinalIgnoreCase);
    private static string Normalize(string value) => value.StartsWith('.') ? value.ToLowerInvariant() : "." + value.ToLowerInvariant();
}
