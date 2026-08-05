namespace GWGUI.Domain.Formats;

public sealed record ImageExtension(string Extension, string DisplayName, bool IsDefault = false);

public sealed record DiskFormat(
    string Id,
    string Family,
    string DisplayName,
    IReadOnlyList<ImageExtension> Extensions,
    bool IsCommon = true,
    IReadOnlySet<string>? CompatibleSourceExtensions = null,
    string? Tag = null);

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
    public IReadOnlyList<DiskFormat> Formats { get; }

    public BuiltInImageFormatCatalog(Func<string, string>? localize = null)
    {
        string T(string key, string fallback) => localize?.Invoke(key) ?? fallback;
        ImageExtension E(string extension, string key, string fallback, bool isDefault = false) => new(extension, T(key, fallback), isDefault);
        DiskFormat F(string id, string family, string fallback, IReadOnlyList<ImageExtension> extensions, bool common, string tag, params string[] sources) => new(id, family, T("Format." + id, fallback), extensions, common, Set(sources), tag);
        var ima = new Func<bool, ImageExtension>(isDefault => E(".ima", "Extension.ima", "IMA disk image", isDefault));
        var img = new Func<ImageExtension>(() => E(".img", "Extension.img", "IMG disk image"));
        IReadOnlyList<ImageExtension> Ibm() => [ima(true), img()];
        Formats =
        [
            new("raw.scp", "Raw", T("Format.raw.scp", "Raw flux image"), [E(".scp", "Extension.scp", "SuperCard Pro", true)], Tag: "RAW-SCP"),
            F("amiga.amigados", "Amiga", "AmigaDOS — 880 KiB", [E(".adf", "Extension.adf.amiga", "Amiga Disk File", true)], true, "AMIGA-DD", ".scp", ".adf", ".hfe"),
            F("amiga.amigados_hd", "Amiga", "AmigaDOS HD — 1.76 MiB", [E(".adf", "Extension.adf.amiga", "Amiga Disk File", true)], true, "AMIGA-HD", ".scp", ".adf", ".hfe"),
            F("atarist.360", "Atari ST", "Atari ST — 360 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-360", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.400", "Atari ST", "Atari ST — 400 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-400", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.440", "Atari ST", "Atari ST — 440 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-440", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.720", "Atari ST", "Atari ST — 720 KiB", [E(".st", "Extension.st", "Atari ST image", true), E(".msa", "Extension.msa", "Magic Shadow Archiver")], true, "ST-720", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.800", "Atari ST", "Atari ST — 800 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-800", ".scp", ".st", ".msa", ".hfe"),
            F("atarist.880", "Atari ST", "Atari ST — 880 KiB", [E(".st", "Extension.st", "Atari ST image", true)], false, "ST-880", ".scp", ".st", ".msa", ".hfe"),
            F("ibm.160", "IBM PC", "IBM PC — 160 KiB", Ibm(), false, "PC-160", ".scp", ".ima", ".img", ".hfe"), F("ibm.180", "IBM PC", "IBM PC — 180 KiB", Ibm(), false, "PC-180", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.320", "IBM PC", "IBM PC — 320 KiB", Ibm(), false, "PC-320", ".scp", ".ima", ".img", ".hfe"), F("ibm.360", "IBM PC", "IBM PC — 360 KiB", Ibm(), true, "PC-360", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.720", "IBM PC", "IBM PC — 720 KiB", Ibm(), true, "PC-720", ".scp", ".ima", ".img", ".hfe"), F("ibm.800", "IBM PC", "IBM PC — 800 KiB", Ibm(), false, "PC-800", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.1200", "IBM PC", "IBM PC — 1.2 MiB", Ibm(), true, "PC-1200", ".scp", ".ima", ".img", ".hfe"), F("ibm.1440", "IBM PC", "IBM PC — 1.44 MiB", Ibm(), true, "PC-1440", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.1680", "IBM PC", "IBM PC — 1.68 MiB", Ibm(), false, "PC-1680", ".scp", ".ima", ".img", ".hfe"), F("ibm.dmf", "IBM PC", "IBM PC — DMF 1.68 MiB", Ibm(), false, "PC-DMF", ".scp", ".ima", ".img", ".hfe"),
            F("ibm.2880", "IBM PC", "IBM PC — 2.88 MiB", Ibm(), false, "PC-2880", ".scp", ".ima", ".img", ".hfe"), F("ibm.scan", "IBM PC", "IBM PC — FM/MFM scan", Ibm(), false, "PC-SCAN", ".scp", ".hfe"),
            F("commodore.1541", "Commodore", "Commodore 64 — 1541", [E(".d64", "Extension.d64", "Commodore D64 image", true)], true, "C64-1541", ".scp", ".d64", ".hfe"),
            F("acorn.adfs.800", "Acorn", "Acorn ADFS — 800 KiB", [E(".adf", "Extension.adf.acorn", "Acorn Disk File", true)], true, "ACORN-800", ".scp", ".adf", ".hfe"),
            F("raw.hfe", "Raw", "HxC flux image", [E(".hfe", "Extension.hfe", "HxC Floppy Emulator", true)], false, "RAW-HFE", ".scp", ".hfe")
        ];
    }

    public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension)
    {
        var normalized = Normalize(sourceExtension);
        return Formats.Where(format => format.CompatibleSourceExtensions?.Contains(normalized) == true).ToArray();
    }

    private static HashSet<string> Set(params string[] values) => new(values.Select(Normalize), StringComparer.OrdinalIgnoreCase);
    private static string Normalize(string value) => value.StartsWith('.') ? value.ToLowerInvariant() : "." + value.ToLowerInvariant();
}
