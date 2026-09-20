namespace GWGUI.MediaEngine.Formats;

/// <summary>Combines a curated image format catalog with formats discovered at runtime.</summary>
public sealed class RuntimeImageFormatCatalog : IImageFormatCatalog
{
    public RuntimeImageFormatCatalog(IImageFormatCatalog curated, IEnumerable<DiskFormat> runtimeFormats)
    {
        ArgumentNullException.ThrowIfNull(curated);
        ArgumentNullException.ThrowIfNull(runtimeFormats);
        var formats = curated.Formats.ToList();
        var known = formats.Select(format => format.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        formats.AddRange(runtimeFormats.Where(format => known.Add(format.Id)));
        Formats = formats;
    }

    public IReadOnlyList<DiskFormat> Formats { get; }

    public IReadOnlyList<DiskFormat> GetCompatibleOutputs(string sourceExtension)
    {
        var normalized = sourceExtension.StartsWith('.')
            ? sourceExtension.ToLowerInvariant()
            : "." + sourceExtension.ToLowerInvariant();
        return Formats.Where(format => format.CompatibleSourceExtensions?.Contains(normalized) == true).ToArray();
    }
}
