using GWGUI.Domain.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.App.Contracts.ViewModels.Conversion;
using GWGUI.App.Enums.ViewModels.Conversion;

namespace GWGUI.App.Presenters.Conversion;

public sealed class ConversionFormatPresenter
{
    public IReadOnlyList<ConversionFormatPresentation> Build(
        IImageFormatCatalog catalog,
        string? sourceExtension,
        DetectedImageFormat? detection,
        IReadOnlySet<string> selectedFormats,
        IReadOnlyDictionary<string, HashSet<string>> explicitExtensions)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var compatible = ConversionSourceCompatibility.GetOutputs(catalog, sourceExtension, detection)
            .Select(format => format.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return catalog.Formats.Select(format =>
            {
                var isCompatible = compatible.Contains(format.Id);
                var isSelected = isCompatible && selectedFormats.Contains(format.Id);
                var group = isSelected ? ConversionFormatGroup.Selected : format.IsCommon ? ConversionFormatGroup.Common : ConversionFormatGroup.Rare;
                IReadOnlySet<string> extensions = explicitExtensions.TryGetValue(format.Id, out var values)
                    ? values.ToHashSet(StringComparer.OrdinalIgnoreCase)
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var isReconstructedFlux = format.Id == "raw.scp" && detection?.Format is not null && sourceExtension is not null && !sourceExtension.Equals(".scp", StringComparison.OrdinalIgnoreCase) && !sourceExtension.Equals(".hfe", StringComparison.OrdinalIgnoreCase);
                return new ConversionFormatPresentation(format, isCompatible, isSelected, extensions, group, isReconstructedFlux);
            })
            .OrderBy(item => item.Group)
            .ThenBy(item => item.Format.DisplayName, StringComparer.CurrentCulture)
            .ToArray();
    }
}
