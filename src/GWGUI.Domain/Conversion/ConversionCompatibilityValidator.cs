using GWGUI.Domain.Formats;
namespace GWGUI.Domain.Conversion;

internal sealed class ConversionCompatibilityValidator(IImageFormatCatalog catalog)
{
    public DiskFormat ResolveFormat(string sourceExtension, string formatId)
    {
        var compatible = catalog.GetCompatibleOutputs(sourceExtension).ToDictionary(format => format.Id);
        return compatible.TryGetValue(formatId, out var format)
            ? format
            : throw new InvalidOperationException($"Format '{formatId}' is incompatible with '{sourceExtension}'.");
    }

    public IReadOnlyList<ImageExtension> ResolveExtensions(DiskFormat format, ConversionSelection selection)
    {
        var requested = selection.ExplicitExtensions.Count == 0
            ? format.Extensions.Where(extension => extension.IsDefault).Select(extension => extension.Extension)
            : selection.ExplicitExtensions;
        return requested.Select(extension =>
            format.Extensions.FirstOrDefault(candidate =>
                string.Equals(candidate.Extension, extension, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Extension '{extension}' is not valid for '{format.DisplayName}'."))
            .ToArray();
    }

    public static void EnsureDistinctOutputs(IReadOnlyList<ConversionOutput> outputs)
    {
        var duplicate = outputs
            .GroupBy(output => output.OutputPath, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException($"Several conversions would create '{duplicate.Key}'.");
    }
}
