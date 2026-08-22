using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
namespace GWGUI.App.Services.DiskImages;

/// <summary>
/// Owns the effective image-format catalog used by the application.
/// Greaseweazle capabilities and optional disk-definitions are combined here,
/// while UI controls remain responsible only for presenting the resulting catalog.
/// </summary>
public sealed class ImageFormatWorkspace
{
    private static readonly IReadOnlySet<string> FallbackImageExtensions =
        new HashSet<string>([".scp", ".img", ".ima", ".hfe"], StringComparer.OrdinalIgnoreCase);

    private readonly Func<string, string> _localize;

    public ImageFormatWorkspace(Func<string, string> localize)
    {
        _localize = localize;
        Rebuild();
    }

    public GwFormatCapabilities Capabilities { get; private set; } = GwFormatCapabilities.Unknown;
    public IImageFormatCatalog Catalog { get; private set; } = null!;
    public ImageFormatDetector Detector { get; private set; } = null!;

    public void SetCapabilities(GwFormatCapabilities capabilities)
    {
        Capabilities = capabilities;
        Rebuild();
    }

    public void AddDiskDefinitions(string path)
    {
        var discovered = DiskDefsFormatReader.Read(path);
        var formatIds = Capabilities.FormatIds.Concat(discovered).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var extensions = Capabilities.ImageExtensions.Count > 0
            ? Capabilities.ImageExtensions
            : FallbackImageExtensions;

        Capabilities = new GwFormatCapabilities(formatIds, extensions);
        Rebuild();
    }

    public void Rebuild()
    {
        Catalog = new CapabilityAwareImageFormatCatalog(
            new BuiltInImageFormatCatalog(_localize),
            Capabilities);
        Detector = new ImageFormatDetector(Catalog);
    }
}
