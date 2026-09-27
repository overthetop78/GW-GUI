namespace GWGUI.MediaEngine.Images.Formats;

/// <summary>Combines a curated image format catalog with formats discovered at runtime.</summary>
public sealed class RuntimeImageFormatCatalog : IImageFormatCatalog
{
    public RuntimeImageFormatCatalog(IImageFormatCatalog curated, IEnumerable<DiskFormat> runtimeFormats)
    {
        ArgumentNullException.ThrowIfNull(curated);
        ArgumentNullException.ThrowIfNull(runtimeFormats);
        var formats = curated.Formats.ToList();
        foreach (var runtime in runtimeFormats)
        {
            var index = formats.FindIndex(format => format.Id.Equals(runtime.Id, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                formats.Add(runtime);
                continue;
            }

            var presentation = formats[index];
            formats[index] = presentation with
            {
                Extensions = runtime.Extensions,
                CompatibleSourceExtensions = runtime.CompatibleSourceExtensions
            };
        }
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
