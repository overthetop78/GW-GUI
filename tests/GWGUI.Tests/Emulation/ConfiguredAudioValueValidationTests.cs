using System.Globalization;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class ConfiguredAudioValueValidationTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("invalid", true)]
    [InlineData("2147483648", true)]
    [InlineData("0", true)]
    [InlineData("-25", true)]
    [InlineData("25", true)]
    [InlineData("1000", true)]
    [InlineData("20", false)]
    [InlineData("50", false)]
    [InlineData(" +050 ", false)]
    [InlineData("200", false)]
    [InlineData("1,000", true)]
    public void ConfiguredLatencyMustBeAnInvariantIntegerFromTheSupportedChoices(string? value, bool invalid)
    {
        Assert.Equal(invalid, EmulationAudioFunctions.HasInvalidConfiguredLatency(value));
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("invalid", true)]
    [InlineData("2147483648", true)]
    [InlineData("-2147483649", true)]
    [InlineData("-1", true)]
    [InlineData("101", true)]
    [InlineData("0", false)]
    [InlineData("100", false)]
    [InlineData(" +037 ", false)]
    [InlineData("37.0", true)]
    [InlineData("1,0", true)]
    public void VolumeBoundsAreInclusiveWithoutClamping(string? value, bool invalid)
    {
        Assert.Equal(invalid, EmulationAudioFunctions.HasInvalidConfiguredVolume(value, 0, 100));
    }

    [Fact]
    public void VolumeUsesTheCallersBoundsAndHandlesIntegerExtremes()
    {
        Assert.False(EmulationAudioFunctions.HasInvalidConfiguredVolume("-25", -50, 50));
        Assert.True(EmulationAudioFunctions.HasInvalidConfiguredVolume("75", -50, 50));
        Assert.False(EmulationAudioFunctions.HasInvalidConfiguredVolume("-2147483648", int.MinValue, int.MaxValue));
        Assert.False(EmulationAudioFunctions.HasInvalidConfiguredVolume("2147483647", int.MinValue, int.MaxValue));
        Assert.True(EmulationAudioFunctions.HasInvalidConfiguredVolume("50", 100, 0));
    }

    [Fact]
    public void AmbientCultureDoesNotChangeConfiguredValueValidation()
    {
        var previous = CultureInfo.CurrentCulture;
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "~";
        try
        {
            CultureInfo.CurrentCulture = culture;
            Assert.False(EmulationAudioFunctions.HasInvalidConfiguredLatency("+50"));
            Assert.False(EmulationAudioFunctions.HasInvalidConfiguredVolume("-25", -50, 50));
            Assert.True(EmulationAudioFunctions.HasInvalidConfiguredVolume("~25", -50, 50));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
