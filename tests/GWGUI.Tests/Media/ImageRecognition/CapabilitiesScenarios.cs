using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.App.Presenters.Conversion;
namespace GWGUI.Tests.Media.ImageRecognition;
internal static class CapabilitiesScenarios
{
    public static void Capabilities()
    {
        var curated = RecognitionEvidenceScenarios.Catalog;
        Assert.Same(curated.Formats, new CapabilityAwareImageFormatCatalog(curated, GwFormatCapabilities.Unknown).Formats);
        var catalog = new CapabilityAwareImageFormatCatalog(curated, new(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "synthetic.new" }, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".img", ".scp" }));
        Assert.Contains(catalog.Formats, x => x.Id == "raw.scp"); Assert.DoesNotContain(catalog.Formats, x => x.Extensions.Any(e => e.Extension == ".adf"));
        var discovered = Assert.Single(catalog.Formats, x => x.Id == "synthetic.new");
        Assert.Equal(".img", Assert.Single(discovered.Extensions).Extension); Assert.True(discovered.Extensions[0].IsDefault);
        Assert.Contains(discovered, catalog.GetCompatibleOutputs("SCP"));
        Assert.All(catalog.Formats, x => Assert.Single(x.Extensions, e => e.IsDefault));
    }
    public static void Presentation()
    {
        var catalog = RecognitionEvidenceScenarios.Catalog; var presenter = new ConversionFormatPresenter();
        var selected = new HashSet<string> { "amiga.amigados", "atarist.720" }; var explicitExtensions = new Dictionary<string, HashSet<string>> { ["amiga.amigados"] = new() { ".adf" } };
        var detection = new ImageFormatDetector(catalog, _ => throw new InvalidOperationException()).Detect("virtual.adf", 901120);
        var items = presenter.Build(catalog, ".adf", detection, selected, explicitExtensions);
        var amiga = Assert.Single(items, x => x.Format.Id == "amiga.amigados"); Assert.True(amiga.IsCompatible); Assert.True(amiga.IsSelected);
        Assert.True(Assert.Single(items, x => x.Format.Id == "raw.scp").IsReconstructedFlux);
        Assert.False(Assert.Single(items, x => x.Format.Id == "atarist.720").IsSelected);
        Assert.All(presenter.Build(catalog, ".unsupported", null, selected, explicitExtensions), x => { Assert.False(x.IsCompatible); Assert.False(x.IsSelected); });
        Assert.Equal(2, selected.Count); Assert.Equal(".adf", Assert.Single(explicitExtensions["amiga.amigados"]));
    }
}
