using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Conversion;

public sealed class ConversionPlanner(IImageFormatCatalog catalog)
{
    public IReadOnlyList<ConversionOutput> Plan(string sourcePath, string destinationFolder, string outputBaseName, IEnumerable<ConversionSelection> selections, bool addTags, string tagPattern = "[{FAMILY}-{FORMAT}] ")
    {
        var sourceExtension = Path.GetExtension(sourcePath);
        var compatible = catalog.GetCompatibleOutputs(sourceExtension).ToDictionary(x => x.Id);
        var outputs = new List<ConversionOutput>();

        foreach (var selection in selections)
        {
            if (!compatible.TryGetValue(selection.FormatId, out var format))
                throw new InvalidOperationException($"Format '{selection.FormatId}' is incompatible with '{sourceExtension}'.");
            var extensions = selection.ExplicitExtensions.Count == 0
                ? format.Extensions.Where(x => x.IsDefault).Select(x => x.Extension)
                : selection.ExplicitExtensions;
            foreach (var extension in extensions)
            {
                var known = format.Extensions.FirstOrDefault(x => string.Equals(x.Extension, extension, StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Extension '{extension}' is not valid for '{format.DisplayName}'.");
                var tag = addTags ? ConversionTagFormatter.Format(tagPattern, format, known.Extension, outputBaseName, DateTime.Now) : "";
                var fileName = !addTags ? outputBaseName
                    : tagPattern.Contains("{NAME}", StringComparison.OrdinalIgnoreCase) ? tag
                    : tag + outputBaseName;
                var outputPath = Path.Combine(destinationFolder, fileName + known.Extension);
                outputs.Add(new ConversionOutput(format.Id, known.Extension, outputPath, selection.ExplicitExtensions.Count == 0));
            }
        }

        var duplicate = outputs.GroupBy(x => x.OutputPath, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null) throw new InvalidOperationException($"Several conversions would create '{duplicate.Key}'.");
        return outputs;
    }

    public static string FormatTag(string pattern, DiskFormat format, string extension, string sourceName, DateTime timestamp) =>
        ConversionTagFormatter.Format(pattern, format, extension, sourceName, timestamp);
}
