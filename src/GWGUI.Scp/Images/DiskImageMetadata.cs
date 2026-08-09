using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

public sealed record DiskImageMetadata(string SystemName, string? ProtectionName)
{
    public static DiskImageMetadata From(SectorImage image, IEnumerable<string>? detectedFormatIds = null)
    {
        var ids = new[] { image.FormatId }.Concat(detectedFormatIds ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var systems = ids.Select(SystemFor).Where(value => value != "\u2014").Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var protection = ids.Any(id => id.Equals("apple2.rwts18", StringComparison.OrdinalIgnoreCase))
            ? "Brøderbund RWTS18"
            : null;
        return new(systems.Length == 0 ? "\u2014" : string.Join(" + ", systems), protection);
    }

    private static string SystemFor(string formatId)
    {
        var id = formatId.ToLowerInvariant();
        return id switch
        {
            var value when value.StartsWith("apple2.") => "Apple II",
            var value when value.StartsWith("apple3.") => "Apple III",
            var value when value.StartsWith("mac.") => "Apple Macintosh",
            var value when value.StartsWith("lisa.") => "Apple Lisa",
            var value when value.StartsWith("amiga.") => "Amiga",
            var value when value.StartsWith("atarist.") => "Atari ST",
            var value when value.StartsWith("atari.") => "Atari 8-bit",
            var value when value.StartsWith("ibm.") => "IBM PC",
            var value when value.StartsWith("commodore.") => "Commodore",
            var value when value.StartsWith("amstrad.") => "Amstrad",
            var value when value.StartsWith("acorn.") || value.StartsWith("bbc.") => "Acorn / BBC Micro",
            var value when value.StartsWith("msx.") => "MSX",
            var value when value.StartsWith("dec.") => "DEC",
            var value when value.StartsWith("coherent.") => "COHERENT",
            var value when value.StartsWith("ucsd.") => "UCSD p-System",
            _ => "\u2014"
        };
    }
}
