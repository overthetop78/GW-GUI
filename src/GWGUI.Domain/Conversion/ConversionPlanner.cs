using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Conversion;

public sealed record ConversionSelection(string FormatId, IReadOnlySet<string> ExplicitExtensions);
public sealed record ConversionOutput(string FormatId, string Extension, string OutputPath, bool UsesImplicitExtension);

public sealed class ConversionPlanner(IImageFormatCatalog catalog)
{
    public IReadOnlyList<ConversionOutput> Plan(string sourcePath, string destinationFolder, string outputBaseName, IEnumerable<ConversionSelection> selections, bool addTags)
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
                var tag = addTags ? $" [{Tag(format)}]" : "";
                var outputPath = Path.Combine(destinationFolder, outputBaseName + tag + known.Extension);
                outputs.Add(new ConversionOutput(format.Id, known.Extension, outputPath, selection.ExplicitExtensions.Count == 0));
            }
        }

        var duplicate = outputs.GroupBy(x => x.OutputPath, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null) throw new InvalidOperationException($"Several conversions would create '{duplicate.Key}'.");
        return outputs;
    }

    private static string Tag(DiskFormat format) => format.DisplayName.ToUpperInvariant().Replace(" — ", "-").Replace(' ', '-');
}
