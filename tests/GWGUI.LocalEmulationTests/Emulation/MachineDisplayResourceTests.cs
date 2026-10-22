using GWGUI.Emulation;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class MachineDisplayResourceTests
{
    [Theory]
    [InlineData("A", "upper")]
    [InlineData("a", "lower")]
    public void LookupKeepsOrdinalIdentifiers(string id, string expected)
    {
        Assert.Equal(expected, EmulationSdk.Adapters.DisplayResourceKey(
            [new("A", "upper"), new("a", "lower")], new Configuration(() => id)));
    }

    [Fact]
    public void SearchReadsConfigurationForEveryEntryAfterFindingAMatch()
    {
        var reads = 0;
        var result = EmulationSdk.Adapters.DisplayResourceKey(
            [new("A", "first"), new("B", "second"), new("C", "third")],
            new Configuration(() => { reads++; return "B"; }));
        Assert.Equal("second", result);
        Assert.Equal(3, reads);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("a")]
    public void MissingOrDifferentlyCasedIdentifierKeepsSingleFailure(string id)
    {
        Assert.Throws<InvalidOperationException>(() => EmulationSdk.Adapters.DisplayResourceKey(
            [new("A", "resource")], new Configuration(() => id)));
    }

    [Fact]
    public void EmptyCatalogDoesNotReadNullConfiguration()
    {
        Assert.Throws<InvalidOperationException>(() => EmulationSdk.Adapters.DisplayResourceKey([], null!));
    }

    [Fact]
    public void NullCatalogKeepsLinqSourceErrorBeforeReadingConfiguration()
    {
        var error = Assert.Throws<ArgumentNullException>(() =>
            EmulationSdk.Adapters.DisplayResourceKey(null!, null!));
        Assert.Equal("source", error.ParamName);
    }

    [Fact]
    public void DuplicateIdentifiersKeepSingleFailure()
    {
        Assert.Throws<InvalidOperationException>(() => EmulationSdk.Adapters.DisplayResourceKey(
            [new("A", "first"), new("A", "second")], new Configuration(() => "A")));
    }

    [Fact]
    public void GetterIsNotCachedBeforeThePredicate()
    {
        var reads = 0;
        Assert.Throws<InvalidOperationException>(() => EmulationSdk.Adapters.DisplayResourceKey(
            [new("A", "first"), new("B", "second")],
            new Configuration(() => ++reads == 1 ? "A" : "B")));
        Assert.Equal(2, reads);
    }

    [Fact]
    public void GetterFailureIsPropagatedUnchanged()
    {
        var error = new InvalidOperationException("model getter");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            EmulationSdk.Adapters.DisplayResourceKey([new("A", "resource")], new Configuration(() => throw error))));
    }

    private sealed class Configuration(Func<string> readMachineId) : IEmulationConfiguration
    {
        public string MachineId => readMachineId();
        public string ModuleId => throw new InvalidOperationException("Unused module identifier");
        public Guid Id => throw new InvalidOperationException("Unused configuration identifier");
    }
}
