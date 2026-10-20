using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class MediaLedMappingTests
{
    [Theory]
    [InlineData(0, EmulationMediaCategory.FloppyDrive)]
    [InlineData(1, EmulationMediaCategory.CassetteDrive)]
    [InlineData(2, EmulationMediaCategory.CartridgeSlot)]
    [InlineData(3, EmulationMediaCategory.CompactDiscDrive)]
    public void CdSnapshotKeepsTheOriginalSlotForEachLed(int led, EmulationMediaCategory category)
    {
        var result = EmulationMediaRules.FromLedStatesWithCd(new Dictionary<int, bool> { [led] = true });
        Assert.Equal(4, result.Count);
        Assert.Equal(new EmulationMediaSlot(category, 0), Assert.Single(result, item => item.Value).Key);
    }

    [Fact]
    public void OriginalSnapshotKeepsThreeKeysAndIgnoresCd()
    {
        var result = EmulationMediaRules.FromLedStates(new Dictionary<int, bool> { [0] = true, [3] = true });
        Assert.Equal(new[] { EmulationMediaSlot.Floppy0, EmulationMediaSlot.Cassette0, EmulationMediaSlot.Cartridge0 }, result.Keys);
        Assert.True(result[EmulationMediaSlot.Floppy0]);
        Assert.False(result.ContainsKey(EmulationMediaSlot.Cd0));
    }

    [Fact]
    public void MissingValuesStayFalseAndUnknownIndicesAreIgnored()
    {
        var result = EmulationMediaRules.FromLedStatesWithCd(new Dictionary<int, bool> { [99] = true });
        Assert.Equal(4, result.Count);
        Assert.All(result.Values, value => Assert.False(value));
    }

    [Fact]
    public void SnapshotsRemainIndependentOfTheirSourceAndEachOther()
    {
        var source = new Dictionary<int, bool> { [0] = false };
        var first = EmulationMediaRules.FromLedStatesWithCd(source);
        source[0] = true;
        var second = EmulationMediaRules.FromLedStatesWithCd(source);
        Assert.NotSame(first, second);
        Assert.False(first[EmulationMediaSlot.Floppy0]);
        Assert.True(second[EmulationMediaSlot.Floppy0]);
        ((IDictionary<EmulationMediaSlot, bool>)second)[EmulationMediaSlot.Floppy0] = false;
        Assert.True(source[0]);
        Assert.True(EmulationMediaRules.FromLedStatesWithCd(source)[EmulationMediaSlot.Floppy0]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LookupsKeepTheirOrderWithoutEnumeratingTheInput(bool withCd)
    {
        var input = new Probe(_ => false);
        _ = Snapshot(input, withCd);
        Assert.Equal(withCd ? new[] { 0, 1, 2, 3 } : new[] { 0, 1, 2 }, input.Reads);
    }

    [Fact]
    public void FailureStopsTheSequenceAndKeepsTheOriginalException()
    {
        var error = new InvalidOperationException("LED lookup");
        var input = new Probe(key => key == 2 ? throw error : true);
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => Snapshot(input, true)));
        Assert.Equal(new[] { 0, 1, 2 }, input.Reads);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NullInputKeepsTheDictionaryExtensionFailure(bool withCd)
    {
        var error = Assert.Throws<ArgumentNullException>(() => Snapshot(null!, withCd));
        Assert.Equal("dictionary", error.ParamName);
    }

    private static IReadOnlyDictionary<EmulationMediaSlot, bool> Snapshot(IReadOnlyDictionary<int, bool> input, bool withCd) =>
        withCd ? EmulationMediaRules.FromLedStatesWithCd(input) : EmulationMediaRules.FromLedStates(input);

    private sealed class Probe(Func<int, bool> read) : IReadOnlyDictionary<int, bool>
    {
        public List<int> Reads { get; } = [];
        public bool TryGetValue(int key, out bool value)
        {
            Reads.Add(key);
            value = read(key);
            return true;
        }
        public bool this[int key] => throw new InvalidOperationException("Unused indexer");
        public IEnumerable<int> Keys => throw new InvalidOperationException("Unused keys");
        public IEnumerable<bool> Values => throw new InvalidOperationException("Unused values");
        public int Count => throw new InvalidOperationException("Unused count");
        public bool ContainsKey(int key) => throw new InvalidOperationException("Unused key check");
        public IEnumerator<KeyValuePair<int, bool>> GetEnumerator() => throw new InvalidOperationException("Unused enumeration");
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
