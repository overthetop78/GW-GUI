using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class StateIdentityComparisonTests
{
    [Fact]
    public void MissingLegacyValuesMatchOnlyAnEmptyIdentity()
    {
        Assert.True(EmulationStateFileFunctions.IdentityValuesEqual(null, new Dictionary<string, string>()));
        Assert.False(EmulationStateFileFunctions.IdentityValuesEqual(null, new Dictionary<string, string> { ["mode"] = "accurate" }));
    }

    [Fact]
    public void OrderDoesNotChangeIdentityAndDictionariesRemainIntact()
    {
        var saved = new Dictionary<string, string> { ["mode"] = "accurate", ["region"] = "pal" };
        var current = new Dictionary<string, string> { ["region"] = "pal", ["mode"] = "accurate" };
        Assert.True(EmulationStateFileFunctions.IdentityValuesEqual(saved, current));
        Assert.Equal(new[] { "mode", "region" }, saved.Keys);
        Assert.Equal(new[] { "region", "mode" }, current.Keys);
    }

    [Theory]
    [InlineData("accurate", "accurate", true)]
    [InlineData("accurate", "Accurate", false)]
    [InlineData(null, null, true)]
    [InlineData(null, "", false)]
    public void ValuesKeepTheirOriginalOrdinalAndNullComparison(string? saved, string? current, bool expected)
    {
        Assert.Equal(expected, EmulationStateFileFunctions.IdentityValuesEqual(
            new Dictionary<string, string> { ["mode"] = saved! },
            new Dictionary<string, string> { ["mode"] = current! }));
    }

    [Fact]
    public void EqualCountsDoNotHideDifferentKeysOrMissingEntries()
    {
        var saved = new Dictionary<string, string> { ["mode"] = "accurate" };
        Assert.False(EmulationStateFileFunctions.IdentityValuesEqual(saved, new Dictionary<string, string>()));
        Assert.False(EmulationStateFileFunctions.IdentityValuesEqual(saved,
            new Dictionary<string, string> { ["other"] = "accurate" }));
    }

    [Fact]
    public void LookupKeepsTheCurrentDictionaryComparerRatherThanSortingOrNormalizingKeys()
    {
        var saved = new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "accurate" };
        var current = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["MODE"] = "accurate" };
        Assert.True(EmulationStateFileFunctions.IdentityValuesEqual(saved, current));
        Assert.False(EmulationStateFileFunctions.IdentityValuesEqual(current, saved));
    }
}
