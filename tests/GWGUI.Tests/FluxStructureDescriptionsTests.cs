using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Exploration.Results;

namespace GWGUI.Tests;

public sealed class FluxStructureDescriptionsTests
{
    [Theory]
    [InlineData(true, "Valid", "checksum valid")]
    [InlineData(false, "Invalid", "checksum invalid")]
    [InlineData(null, "Unavailable", "checksum unavailable")]
    public void IntegrityDescriptionsRepresentEveryState(bool? valid, string expectedState, string expectedDescription)
    {
        Assert.Equal(expectedState, FluxStructureDescriptions.IntegrityState(valid).ToString());
        Assert.Equal(expectedDescription, FluxStructureDescriptions.Integrity("checksum", valid));
    }

    [Fact]
    public void CompleteDescriptionContainsEveryInjectedValue()
    {
        var description = FluxStructureDescriptions.Complete("Codec", FluxStructureKind.FormatHeader, 12, 1, 7, 512, 0xFE, "variant", true, false, "header checksum", "data checksum");

        Assert.Equal("Codec FormatHeader, C12 H1 R7, 512 bytes, mark FE, variant, header checksum valid, data checksum invalid", description);
    }

    [Fact]
    public void TruncatedDescriptionContainsEveryInjectedValue()
    {
        var description = FluxStructureDescriptions.Truncated("Codec", FluxStructureKind.FormatData, 0xFB, "variant");

        Assert.Equal("Codec FormatData, mark FB, variant, truncated", description);
    }

    [Fact]
    public void UnpairedDescriptionContainsEveryInjectedValue()
    {
        var description = FluxStructureDescriptions.UnpairedData("Codec", 0xF8, "variant");

        Assert.Equal("Unpaired Codec data, mark F8, variant", description);
    }

    [Fact]
    public void UnclassifiedDescriptionContainsEveryInjectedValue()
    {
        var description = FluxStructureDescriptions.UnclassifiedMark("Codec", FluxStructureKind.TimingAnomaly, 0xA1, "variant");

        Assert.Equal("Unclassified Codec TimingAnomaly, mark A1, variant", description);
    }
}
