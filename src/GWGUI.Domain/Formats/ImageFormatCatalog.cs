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

public sealed class CapabilityAwareImageFormatCatalog : IImageFormatCatalog
{
    public IReadOnlyList<DiskFormat> Formats { get; }

    public CapabilityAwareImageFormatCatalog(IImageFormatCatalog curated, GwFormatCapabilities capabilities)
    {
        if (!capabilities.IsKnown)
        {
            Formats = curated.Formats;
            return;
        }

        Formats = curated.Formats
            .Where(format => format.Id == "raw.scp" || capabilities.FormatIds.Contains(format.Id))
            .Select(format => FilterExtensions(format, capabilities.ImageExtensions))
            .Where(format => format.Extensions.Count > 0)
            .ToArray();
    }

    public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension)
    {
        var normalized = Normalize(sourceExtension);
        return Formats.Where(format => format.CompatibleSourceExtensions?.Contains(normalized) == true).ToArray();
    }

    private static DiskFormat FilterExtensions(DiskFormat format, IReadOnlySet<string> supported)
    {
        var extensions = format.Extensions.Where(extension => supported.Contains(Normalize(extension.Extension))).ToArray();
        if (extensions.Length == 0) return format with { Extensions = [] };
        if (extensions.Any(extension => extension.IsDefault)) return format with { Extensions = extensions };
        return format with { Extensions = extensions.Select((extension, index) => extension with { IsDefault = index == 0 }).ToArray() };
    }

    private static string Normalize(string value) => value.StartsWith('.') ? value.ToLowerInvariant() : "." + value.ToLowerInvariant();
}

public sealed class BuiltInImageFormatCatalog : IImageFormatCatalog
{
    public IReadOnlyList<DiskFormat> Formats { get; } =
    [
        new("raw.scp", "Raw", "Image brute", [new(".scp", "SuperCard Pro", true)]),
        new("amiga.amigados", "Amiga", "AmigaDOS — 880 Kio", [new(".adf", "Amiga Disk File", true)], CompatibleSourceExtensions: Set(".scp", ".adf", ".hfe")),
        new("amiga.amigados_hd", "Amiga", "AmigaDOS HD — 1,76 Mio", [new(".adf", "Amiga Disk File", true)], CompatibleSourceExtensions: Set(".scp", ".adf", ".hfe")),
        new("atarist.360", "Atari ST", "Atari ST — 360 Kio", [new(".st", "Image Atari ST", true)], CompatibleSourceExtensions: Set(".scp", ".st", ".msa", ".hfe"), IsCommon: false),
        new("atarist.400", "Atari ST", "Atari ST — 400 Kio", [new(".st", "Image Atari ST", true)], CompatibleSourceExtensions: Set(".scp", ".st", ".msa", ".hfe"), IsCommon: false),
        new("atarist.440", "Atari ST", "Atari ST — 440 Kio", [new(".st", "Image Atari ST", true)], CompatibleSourceExtensions: Set(".scp", ".st", ".msa", ".hfe"), IsCommon: false),
        new("atarist.720", "Atari ST", "Atari ST — 720 Kio", [new(".st", "Image Atari ST", true), new(".msa", "Magic Shadow Archiver")], CompatibleSourceExtensions: Set(".scp", ".st", ".msa", ".hfe")),
        new("atarist.800", "Atari ST", "Atari ST — 800 Kio", [new(".st", "Image Atari ST", true)], CompatibleSourceExtensions: Set(".scp", ".st", ".msa", ".hfe"), IsCommon: false),
        new("atarist.880", "Atari ST", "Atari ST — 880 Kio", [new(".st", "Image Atari ST", true)], CompatibleSourceExtensions: Set(".scp", ".st", ".msa", ".hfe"), IsCommon: false),
        new("ibm.160", "IBM PC", "IBM PC — 160 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe"), IsCommon: false),
        new("ibm.180", "IBM PC", "IBM PC — 180 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe"), IsCommon: false),
        new("ibm.320", "IBM PC", "IBM PC — 320 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe"), IsCommon: false),
        new("ibm.360", "IBM PC", "IBM PC — 360 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe")),
        new("ibm.720", "IBM PC", "IBM PC — 720 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe")),
        new("ibm.800", "IBM PC", "IBM PC — 800 Kio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe"), IsCommon: false),
        new("ibm.1200", "IBM PC", "IBM PC — 1,2 Mio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe")),
        new("ibm.1440", "IBM PC", "IBM PC — 1,44 Mio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe")),
        new("ibm.1680", "IBM PC", "IBM PC — 1,68 Mio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe"), IsCommon: false),
        new("ibm.dmf", "IBM PC", "IBM PC — DMF 1,68 Mio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe"), IsCommon: false),
        new("ibm.2880", "IBM PC", "IBM PC — 2,88 Mio", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".ima", ".img", ".hfe"), IsCommon: false),
        new("ibm.scan", "IBM PC", "IBM PC — Recherche FM/MFM", [new(".ima", "Image disque IMA", true), new(".img", "Image disque IMG")], CompatibleSourceExtensions: Set(".scp", ".hfe"), IsCommon: false),
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
