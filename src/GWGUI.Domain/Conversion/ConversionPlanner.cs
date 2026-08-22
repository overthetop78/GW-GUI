using GWGUI.Domain.Formats;
namespace GWGUI.Domain.Conversion;

public sealed class ConversionPlanner(IImageFormatCatalog catalog)
{
    public IReadOnlyList<ConversionOutput> Plan(string sourcePath, string destinationFolder, string outputBaseName, IEnumerable<ConversionSelection> selections, bool addTags, string tagPattern = "[{FAMILY}-{FORMAT}] ")
    {
        var sourceExtension = Path.GetExtension(sourcePath);
        var validator = new ConversionCompatibilityValidator(catalog);
        var outputs = new List<ConversionOutput>();

        foreach (var selection in selections)
        {
            var format = validator.ResolveFormat(sourceExtension, selection.FormatId);
            foreach (var extension in validator.ResolveExtensions(format, selection))
                outputs.Add(ConversionOutputFactory.Create(
                    destinationFolder,
                    outputBaseName,
                    format,
                    extension,
                    sourceExtension,
                    addTags,
                    tagPattern,
                    selection.ExplicitExtensions.Count == 0));
        }

        ConversionCompatibilityValidator.EnsureDistinctOutputs(outputs);
        return outputs;
    }

    public static string FormatTag(string pattern, DiskFormat format, string extension, string sourceName, DateTime timestamp) =>
        ConversionTagFormatter.Format(pattern, format, extension, sourceName, timestamp);
}
