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
        Assert.Contains(catalog.Formats, x => x.Id == "raw.scp");
        Assert.Contains(catalog.Formats, x => x.Extensions.Any(e => e.Extension == ".adf"));
        var discovered = Assert.Single(catalog.Formats, x => x.Id == "synthetic.new");
        Assert.Equal(".img", Assert.Single(discovered.Extensions).Extension); Assert.True(discovered.Extensions[0].IsDefault);
        Assert.Contains(discovered, catalog.GetCompatibleOutputs("SCP"));
        Assert.All(catalog.Formats, x => Assert.Single(x.Extensions, e => e.IsDefault));

        var atariCatalog = new CapabilityAwareImageFormatCatalog(
            new BuiltInImageFormatCatalog(),
            new(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "atari.90", "atari.130" },
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".atr" }));
        Assert.Equal(
            ["atari.90", "atari.130", "atari.140", "atari.180"],
            new DiskClassificationCatalog(atariCatalog.Formats)
                .FormatsFor("Atari 8-bit")
                .Select(format => format.Id));
        Assert.All(
            atariCatalog.Formats.Where(format => format.Id is "atari.90" or "atari.130"),
            format =>
            {
                Assert.True(format.SupportsPhysicalRead);
                Assert.True(format.SupportsPhysicalWrite);
            });
        var atari180 = Assert.Single(atariCatalog.Formats, format => format.Id == "atari.180");
        Assert.Equal(FloppyFormFactor.FiveAndQuarterInch, atari180.FormFactor);
        Assert.False(atari180.SupportsPhysicalRead);
        Assert.False(atari180.SupportsPhysicalWrite);
        var atari140 = Assert.Single(atariCatalog.Formats, format => format.Id == "atari.140");
        Assert.Equal(FloppyFormFactor.FiveAndQuarterInch, atari140.FormFactor);
        Assert.False(atari140.SupportsPhysicalRead);
        Assert.False(atari140.SupportsPhysicalWrite);

        var builtInFormats = new BuiltInImageFormatCatalog().Formats;
        Assert.All(
            builtInFormats.Where(format => format.FormFactor == FloppyFormFactor.ThreeAndHalfInch),
            format => Assert.NotEqual(FloppyDensity.Unknown, format.Density));
        Assert.Equal(FloppyDensity.DoubleDensity, Assert.Single(builtInFormats, format => format.Id == "ibm.720").Density);
        Assert.Equal(FloppyDensity.HighDensity, Assert.Single(builtInFormats, format => format.Id == "ibm.1440").Density);
        Assert.Equal(FloppyDensity.ExtendedDensity, Assert.Single(builtInFormats, format => format.Id == "ibm.2880").Density);
        var msxFormats = builtInFormats.Where(format => format.Family == "MSX").ToArray();
        Assert.Equal(4, msxFormats.Length);
        Assert.All(
            msxFormats,
            format =>
            {
                Assert.Equal(FloppyFormFactor.ThreeAndHalfInch, format.FormFactor);
                Assert.Equal(FloppyDensity.DoubleDensity, format.Density);
            });
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
