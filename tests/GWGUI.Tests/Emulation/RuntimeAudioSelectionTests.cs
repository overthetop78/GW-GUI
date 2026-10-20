using System.Globalization;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class RuntimeAudioSelectionTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("default", null)]
    [InlineData("Default", "Default")]
    [InlineData(" default ", " default ")]
    [InlineData("device-id", "device-id")]
    public void OutputPreservesTheStoredValueExceptTheExactDefaultToken(string? stored, string? expected)
    {
        var options = new Dictionary<string, string> { ["family-output"] = stored! };
        Assert.Equal(expected, EmulationAudioFunctions.ResolveRuntimeOutput(options, "family-output", "default"));
    }

    [Fact]
    public void LookupsKeepTheCallersKeysComparerAndDefaultToken()
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Family-Output"] = "auto",
            ["Family-Latency"] = "37",
            ["other-output"] = "another-device"
        };
        Assert.Null(EmulationAudioFunctions.ResolveRuntimeOutput(options, "family-output", "auto"));
        Assert.Equal("auto", EmulationAudioFunctions.ResolveRuntimeOutput(options, "family-output", "default"));
        Assert.Null(EmulationAudioFunctions.ResolveRuntimeOutput(options, "missing", "default"));
        Assert.Equal(37, EmulationAudioFunctions.ResolveRuntimeLatency(options, "family-latency"));
        Assert.Equal(50, EmulationAudioFunctions.ResolveRuntimeLatency(options, "missing"));
        Assert.Equal(3, options.Count);
    }

    [Theory]
    [InlineData(null, 50)]
    [InlineData("", 50)]
    [InlineData(" ", 50)]
    [InlineData("invalid", 50)]
    [InlineData("1,000", 50)]
    [InlineData("1.5", 50)]
    [InlineData("2147483648", 50)]
    [InlineData("-2147483649", 50)]
    [InlineData("-2147483648", int.MinValue)]
    [InlineData("2147483647", int.MaxValue)]
    [InlineData(" +37 ", 37)]
    [InlineData(" -25 ", -25)]
    [InlineData("0", 0)]
    public void LatencyKeepsInvariantIntegerParsingAndDoesNotClamp(string? stored, int expected)
    {
        var options = new Dictionary<string, string> { ["family-latency"] = stored! };
        Assert.Equal(expected, EmulationAudioFunctions.ResolveRuntimeLatency(options, "family-latency"));
    }

    [Fact]
    public void AmbientCultureDoesNotChangeTheInvariantLatencyParser()
    {
        var previous = CultureInfo.CurrentCulture;
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat.NegativeSign = "~";
        try
        {
            CultureInfo.CurrentCulture = culture;
            Assert.Equal(-25, EmulationAudioFunctions.ResolveRuntimeLatency(
                new Dictionary<string, string> { ["latency"] = "-25" }, "latency"));
            Assert.Equal(50, EmulationAudioFunctions.ResolveRuntimeLatency(
                new Dictionary<string, string> { ["latency"] = "~25" }, "latency"));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }
}
