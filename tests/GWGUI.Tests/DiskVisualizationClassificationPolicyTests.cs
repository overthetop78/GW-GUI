using GWGUI.App.Enums.Rendering.Scp;
using GWGUI.App.Functions.Services.Visualization;
using GWGUI.MediaEngine.Exploration.Scp;
namespace GWGUI.Tests;

public sealed class DiskVisualizationClassificationPolicyTests
{
    [Theory]
    [InlineData("Apple II", "apple2.gcr", DiskMediaCategory.FiveQuarterDd)]
    [InlineData("Amiga", "amiga.mfm", DiskMediaCategory.ThreeHalfDd)]
    [InlineData("DEC", "dec.rx02", DiskMediaCategory.EightInch)]
    [InlineData("Amstrad", "iso.mfm", DiskMediaCategory.ThreeInch)]
    public void MachineSelectsDecoderAndMedia(string machine, string decoder, DiskMediaCategory media)
    {
        var result = DiskVisualizationClassificationFunctions.Resolve(machine, null, null, false);

        Assert.Equal(decoder, result.DecoderId);
        Assert.Equal(media, result.MediaCategory);
    }

    [Fact]
    public void ProtectionOverridesMachineDecoder()
    {
        var result = DiskVisualizationClassificationFunctions.Resolve("Apple II", "apple2.appledos.140", "apple2.rwts18", false);

        Assert.Equal("apple2.rwts18", result.DecoderId);
    }

    [Fact]
    public void EmptyAutomaticSelectionLeavesDecoderUnforced()
    {
        var result = DiskVisualizationClassificationFunctions.Resolve(null, null, null, true);

        Assert.Null(result.DecoderId);
        Assert.Equal(DiskMediaCategory.Unknown, result.MediaCategory);
    }

    [Theory]
    [InlineData("ibm.1440")]
    [InlineData("ibm.2880")]
    [InlineData("amiga.amigados_hd")]
    public void HighDensityFormatSelectsHighDensityMedia(string formatId)
    {
        var result = DiskVisualizationClassificationFunctions.Resolve("IBM PC", formatId, null, false);

        Assert.Equal(DiskMediaCategory.ThreeHalfHd, result.MediaCategory);
    }
}
