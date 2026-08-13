using GWGUI.Domain.Conversion;

namespace GWGUI.Tests;

public sealed class ConversionFidelityTests
{
    [Theory]
    [InlineData(".adf")]
    [InlineData(".st")]
    [InlineData(".img")]
    [InlineData(".dsk")]
    public void SectorOutputsNeverClaimToPreserveProtection(string extension)
    {
        var output = new ConversionOutput("test", extension, $"disk{extension}", false);

        Assert.Equal(ConversionFidelityLevel.SectorData, output.Fidelity);
        Assert.False(output.PreservesOriginalProtection);
    }

    [Theory]
    [InlineData(".scp")]
    [InlineData(".hfe")]
    public void EncodedFluxOutputsAreDeclaredAsReconstructed(string extension)
    {
        var output = new ConversionOutput("test", extension, $"disk{extension}", false);

        Assert.Equal(ConversionFidelityLevel.ReconstructedTracks, output.Fidelity);
        Assert.False(output.PreservesOriginalProtection);
    }

    [Fact]
    public void OnlyAnExplicitFluxPreservingPathCanClaimProtectionPreservation()
    {
        var output = new ConversionOutput("raw.scp", ".scp", "copy.scp", false, ConversionFidelityLevel.PreservedFlux);

        Assert.True(output.PreservesOriginalProtection);
    }
}
