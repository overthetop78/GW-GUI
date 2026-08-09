using GWGUI.Domain.Formats;

namespace GWGUI.Domain.Write;

public sealed class ImageFormatDetector(IImageFormatCatalog catalog)
{
    private static readonly IReadOnlyList<IImageFormatDetectionRule> Rules =
    [
        new RawImageFormatDetectionRule(),
        new AdfImageFormatDetectionRule(),
        new AtariImageFormatDetectionRule(),
        new AppleImageFormatDetectionRule(),
        new MacintoshImageFormatDetectionRule(),
        new IbmPcImageFormatDetectionRule()
    ];

    public DetectedImageFormat Detect(string filePath, long? knownLength = null)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var candidates = catalog.Formats
            .Where(format => format.Extensions.Any(item => item.Extension.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        var context = new ImageFormatDetectionContext(catalog, filePath, knownLength, extension, candidates);
        foreach (var rule in Rules)
            if (rule.TryDetect(context, out var result))
                return result;

        if (candidates.Length == 1)
            return new(extension, candidates[0], FormatConfidence.Inferred, candidates, "Detection.ExtensionInferred");
        return new(extension, null, FormatConfidence.Ambiguous, candidates, "Detection.Multiple");
    }

}
