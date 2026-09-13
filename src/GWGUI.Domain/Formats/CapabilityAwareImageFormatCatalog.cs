using System.Globalization;
using System.Text.RegularExpressions;

namespace GWGUI.Domain.Formats;

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

        var curatedFormats = curated.Formats
            .Select(format => ApplyPhysicalCapabilities(format, capabilities))
            .ToList();
        var known = curatedFormats.Select(format => format.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        curatedFormats.AddRange(capabilities.FormatIds
            .Where(id => !known.Contains(id))
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(id => CreateDiscoveredFormat(id, capabilities.ImageExtensions))
            .Where(format => format.Extensions.Count > 0));
        Formats = curatedFormats;
    }

    public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension)
    {
        var normalized = Normalize(sourceExtension);
        return Formats.Where(format => format.CompatibleSourceExtensions?.Contains(normalized) == true).ToArray();
    }

    private static DiskFormat ApplyPhysicalCapabilities(DiskFormat format, GwFormatCapabilities capabilities)
    {
        if (!format.SupportsPhysicalRead && !format.SupportsPhysicalWrite) return format;
        var supported = BuiltInDiskDefinitions.Supports(format.Id);
        var gwFormat = GwFormatArgument.FromCatalogId(format.Id);
        supported |= gwFormat is not null && capabilities.FormatIds.Contains(gwFormat);
        return format with
        {
            SupportsPhysicalRead = format.SupportsPhysicalRead && supported,
            SupportsPhysicalWrite = format.SupportsPhysicalWrite && supported
        };
    }

    private static DiskFormat CreateDiscoveredFormat(string id, IReadOnlySet<string> supported)
    {
        var familyId = id.Split('.', 2)[0];
        var family = FriendlyToken(familyId);
        var detail = id.Contains('.') ? string.Join(" · ", id.Split('.')[1..].Select(FriendlyToken)) : null;
        var preferred = supported.Contains(".img") ? ".img" : supported.Contains(".scp") ? ".scp" : supported.Order(StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        var extensions = preferred is null ? Array.Empty<ImageExtension>() : [new ImageExtension(preferred, preferred.TrimStart('.').ToUpperInvariant(), true)];
        var sources = new HashSet<string>(supported.Select(Normalize), StringComparer.OrdinalIgnoreCase);
        var tag = Regex.Replace(id.ToUpperInvariant(), "[^A-Z0-9]+", "-").Trim('-');
        return new DiskFormat(
            id,
            family,
            detail is null ? family : $"{family} — {detail}",
            extensions,
            false,
            sources,
            tag,
            FloppyFormFactor.Unknown,
            SupportsPhysicalRead: true,
            SupportsPhysicalWrite: true);
    }

    private static string FriendlyToken(string token)
    {
        var known = token.ToLowerInvariant() switch
        {
            "ibm" => "IBM PC", "pc98" => "PC-98", "msx" => "MSX", "dec" => "DEC",
            "hp" => "HP", "rm" => "RM", "tsc" => "TSC", "zx" => "ZX Spectrum",
            "coco" => "TRS-80 CoCo", "apple2" => "Apple II", "atarist" => "Atari ST",
            "rx01" => "RX01", "rx02" => "RX02", "fm" => "FM", "mfm" => "MFM",
            "gcr" => "GCR", "adfs" => "ADFS", "dos" => "DOS", "hd" => "HD", "dd" => "DD",
            _ => null
        };
        if (known is not null) return known;
        var words = Regex.Split(token.Replace('_', '-'), "[-]+").Where(word => word.Length > 0)
            .Select(word => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLowerInvariant()));
        return string.Join(" ", words);
    }

    private static string Normalize(string value) => value.StartsWith('.') ? value.ToLowerInvariant() : "." + value.ToLowerInvariant();
}
