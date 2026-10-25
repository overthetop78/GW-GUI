using GWGUI.Emulation;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class EmulatorOptionSelectionTests
{
    [Fact]
    public void NativeAndHostOptionsKeepInputOrderAndProduceAnIndependentSnapshot()
    {
        var options = new Dictionary<string, string>
        {
            ["foreign"] = "discard",
            ["volume"] = "75",
            ["native"] = "custom",
            ["latency"] = "37",
            ["output"] = "device-id"
        };
        var selected = EmulationSdk.OptionProcessing.FilterForEmulator(["native"], options, "output", "latency", "volume");
        Assert.Equal(new[] { "volume", "native", "latency", "output" }, selected.Keys);
        Assert.Equal("custom", selected["native"]);
        Assert.Equal("device-id", selected["output"]);
        Assert.Equal("37", selected["latency"]);
        Assert.Equal("75", selected["volume"]);
        selected["native"] = "changed-result";
        Assert.Equal("custom", options["native"]);
        options["output"] = "changed-input";
        Assert.Equal("device-id", selected["output"]);
        Assert.True(options.ContainsKey("foreign"));
        Assert.False(selected.ContainsKey("foreign"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SourceComparerDoesNotChangeOrdinalKeySelectionOrTheResultComparer(bool ignoreCase)
    {
        var options = new Dictionary<string, string>(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
        {
            ["KNOWN"] = "wrong-native-case",
            ["Host"] = "wrong-host-case",
            ["native"] = "kept"
        };
        Assert.Equal(ignoreCase, options.ContainsKey("known"));
        var selected = EmulationSdk.OptionProcessing.FilterForEmulator(["known", "native"], options, "host", "latency", "volume");
        Assert.Equal("kept", Assert.Single(selected).Value);
        Assert.True(selected.ContainsKey("native"));
        Assert.False(selected.ContainsKey("NATIVE"));
    }

    [Fact]
    public void DuplicateCatalogKeysDoNotDuplicateRowsOrMergeDifferentCase()
    {
        var options = new Dictionary<string, string> { ["native"] = "lower", ["NATIVE"] = "upper" };
        var selected = EmulationSdk.OptionProcessing.FilterForEmulator(
            ["native", "native", "NATIVE"], options, "output", "latency", "volume");
        Assert.Equal(2, selected.Count);
        Assert.Equal("lower", selected["native"]);
        Assert.Equal("upper", selected["NATIVE"]);
    }

    [Fact]
    public void OverlappingNativeAndHostKeysStillProduceOneRow()
    {
        var selected = EmulationSdk.OptionProcessing.FilterForEmulator(["shared"],
            new Dictionary<string, string> { ["shared"] = "value" }, "shared", "shared", "shared");
        Assert.Equal("value", Assert.Single(selected).Value);
    }

    [Fact]
    public void RetainedValuesAreNotValidatedOrNormalizedByTheFilter()
    {
        var selected = EmulationSdk.OptionProcessing.FilterForEmulator(["null", "empty"],
            new Dictionary<string, string> { ["null"] = null!, ["empty"] = "", ["output"] = null! },
            "output", "latency", "volume");
        Assert.Equal(3, selected.Count);
        Assert.Null(selected["null"]);
        Assert.Equal("", selected["empty"]);
        Assert.Null(selected["output"]);
    }

    [Fact]
    public void CatalogMaterializationCompletesBeforeOptionsAreEnumerated()
    {
        var options = new Dictionary<string, string> { ["foreign"] = "discard" };
        var catalogReads = 0;
        IEnumerable<string> Keys()
        {
            catalogReads++;
            options["added"] = "from-catalog";
            yield return "added";
        }
        var selected = EmulationSdk.OptionProcessing.FilterForEmulator(Keys(), options, "output", "latency", "volume");
        Assert.Equal(1, catalogReads);
        Assert.Equal("from-catalog", Assert.Single(selected).Value);
    }
}
