using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Interfaces;
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
    [Theory]
    [InlineData("core", "1.0", "aBcD", true)]
    [InlineData("Core", "1.0", "ABCD", false)]
    [InlineData("core", "1.1", "ABCD", false)]
    [InlineData("core", "1.0", "FFFF", false)]
    [InlineData(null, "1.0", "ABCD", false)]
    public void CoreMetadataKeepsTheOriginalCaseComparisons(string? name, string version, string hash, bool expected)
    {
        using var core = new IdentityCore();
        Assert.Equal(expected, EmulationStateFileFunctions.CoreIdentityMatches(true, name, version, hash, core));
        Assert.False(core.Disposed);
    }

    [Theory]
    [InlineData(false, "core", "1.0", "ABCD", "")]
    [InlineData(true, "other", "1.0", "ABCD", "name")]
    [InlineData(true, "core", "other", "ABCD", "name,version")]
    [InlineData(true, "core", "1.0", "other", "name,version,hash")]
    [InlineData(true, "core", "1.0", "ABCD", "name,version,hash")]
    public void MetadataStopsAtTheFirstMismatchAndLeavesTheRuntimeOwnedByTheCaller(
        bool sameEmulator, string name, string version, string hash, string expectedReads)
    {
        using var core = new IdentityCore();
        EmulationStateFileFunctions.CoreIdentityMatches(sameEmulator, name, version, hash, core);
        Assert.Equal(expectedReads, string.Join(",", core.Reads));
        Assert.False(core.Disposed);
    }

    private sealed class IdentityCore : IEmulationCoreRuntime<object, object>
    {
        public List<string> Reads { get; } = [];
        public bool Disposed { get; private set; }
        public string CoreName { get { Reads.Add("name"); return "core"; } }
        public string CoreVersion { get { Reads.Add("version"); return "1.0"; } }
        public string CoreSha256 { get { Reads.Add("hash"); return "ABCD"; } }
        public VideoFrame? LatestVideoFrame => null;
        public AudioChunk? LatestAudioChunk => null;
        public IReadOnlyList<object> Options => [];
        public IReadOnlyList<string> Diagnostics => [];
        public IReadOnlyDictionary<int, bool> LedStates => new Dictionary<int, bool>();
        public IReadOnlySet<string> SupportedContentExtensions => new HashSet<string>();
        public double FramesPerSecond => 50;
        public int SampleRate => 44100;
        public bool TryDequeueAudio(out AudioChunk? chunk) { chunk = null; return false; }
        public void Initialize(object configuration, string sessionDirectory, string? saveDirectory = null) { }
        public void RunFrame() { }
        public void SetInput(EmulationInputSnapshot snapshot) { }
        public void Dispose() => Disposed = true;
    }
}
