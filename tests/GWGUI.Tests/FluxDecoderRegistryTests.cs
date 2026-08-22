using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

public sealed class FluxDecoderRegistryTests
{
    [Fact]
    public void LisaFileWareDecoderKeepsItsExplicitIdentityWithoutAutomaticDuplication()
    {
        var revolution = new FluxRevolution(8_000_000, []);
        var decoder = new AppleLisaFileWareGcrDecoder();
        var result = decoder.Decode(revolution);

        Assert.Equal(FluxCodecIds.AppleLisaFileWareGcr, decoder.Id);
        Assert.Equal(FluxCodecDisplayNames.AppleLisaFileWareGcr, decoder.DisplayName);
        Assert.Equal(decoder.Id, result.DecoderId);
        Assert.Equal(decoder.DisplayName, result.DisplayName);
        Assert.NotEqual(FluxCodecIds.AppleLisaFileWareGcr, new FluxDecoderRegistry().DecodeAutomatic(revolution).DecoderId);
    }

    [Fact]
    public void ConstructorRejectsInvalidDecoderCollections()
    {
        Assert.Throws<ArgumentNullException>(() => new FluxDecoderRegistry(null!));
        Assert.Throws<ArgumentException>(() => new FluxDecoderRegistry([]));
        Assert.Throws<ArgumentException>(() => new FluxDecoderRegistry([null!]));
        Assert.Throws<ArgumentException>(() => new FluxDecoderRegistry([new StubDecoder(" ")]));
        Assert.Throws<InvalidOperationException>(() => new FluxDecoderRegistry([new StubDecoder("same"), new StubDecoder("same")]));
    }

    [Fact]
    public void DecodeResolvesEveryCatalogIdentifierAndReportsAnAbsentIdentifier()
    {
        var registry = new FluxDecoderRegistry();
        var revolution = new FluxRevolution(8_000_000, []);

        foreach (var decoder in registry.Decoders) Assert.Equal(decoder.Id, registry.Decode(decoder.Id, revolution).DecoderId);

        var exception = Assert.Throws<KeyNotFoundException>(() => registry.Decode("absent", revolution));
        Assert.Equal("No flux decoder is registered with identifier 'absent'.", exception.Message);
    }

    [Fact]
    public void DecodersCollectionCannotBeModified()
    {
        var decoders = Assert.IsAssignableFrom<IList<IFluxDecoder>>(new FluxDecoderRegistry().Decoders);

        Assert.True(decoders.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => decoders.Add(new StubDecoder("added")));
        Assert.Throws<NotSupportedException>(() => decoders.RemoveAt(0));
        Assert.Throws<NotSupportedException>(() => decoders[0] = new StubDecoder("replaced"));
    }

    [Fact]
    public void DecodeExecutesDecoderOnceForSameIdentifierAndRevolution()
    {
        var decoder = new CountingDecoder("counted");
        var registry = new FluxDecoderRegistry([decoder]);
        var revolution = new FluxRevolution(8_000_000, []);

        var first = registry.Decode(decoder.Id, revolution);
        var second = registry.Decode(decoder.Id, revolution);

        Assert.Same(first, second);
        Assert.Equal(1, decoder.ExecutionCount);
    }

    [Fact]
    public void DecodeUsesDistinctCacheEntriesForAnotherRevolutionOrIdentifier()
    {
        var firstDecoder = new CountingDecoder("first");
        var secondDecoder = new CountingDecoder("second");
        var registry = new FluxDecoderRegistry([firstDecoder, secondDecoder]);
        var firstRevolution = new FluxRevolution(8_000_000, []);
        var secondRevolution = new FluxRevolution(8_000_000, []);

        var first = registry.Decode(firstDecoder.Id, firstRevolution);
        var anotherRevolution = registry.Decode(firstDecoder.Id, secondRevolution);
        var anotherIdentifier = registry.Decode(secondDecoder.Id, firstRevolution);

        Assert.NotSame(first, anotherRevolution);
        Assert.NotSame(first, anotherIdentifier);
        Assert.Equal(2, firstDecoder.ExecutionCount);
        Assert.Equal(1, secondDecoder.ExecutionCount);
    }

    [Fact]
    public async Task ConcurrentDecodeCallsShareSameDeferredResult()
    {
        using var decoder = new BlockingDecoder("blocking");
        var registry = new FluxDecoderRegistry([decoder]);
        var revolution = new FluxRevolution(8_000_000, []);

        var first = Task.Run(() => registry.Decode(decoder.Id, revolution));
        Assert.True(decoder.WaitUntilEntered(TimeSpan.FromSeconds(5)));
        var second = Task.Run(() => registry.Decode(decoder.Id, revolution));
        decoder.Release();

        var results = await Task.WhenAll(first, second);
        Assert.Same(results[0], results[1]);
        Assert.Equal(1, decoder.ExecutionCount);
    }

    [Fact]
    public void ScoringCoversEveryResultBranch()
    {
        var valid = new DecodedSector(0, 0, 1, 0, 128, true, 0);
        var unverified = valid with { IntegrityValid = null };
        var invalid = valid with { IntegrityValid = false };
        var structure = new FluxStructure(FluxStructureKind.Sync, 0, 1, "sync");

        Assert.Equal(4.55, FluxDecoderScoring.Calculate(Result("valid", 0.5, sectors: [valid, invalid])), 10);
        Assert.Equal(3.5, FluxDecoderScoring.Calculate(Result("unverified", 0.5, sectors: [unverified])), 10);
        Assert.Equal(0.005, FluxDecoderScoring.Calculate(Result("invalid", 0.5, sectors: [invalid])), 10);
        Assert.Equal(1.5, FluxDecoderScoring.Calculate(Result(FluxCodecIds.Raw, 0.5)), 10);
        Assert.Equal(2.5, FluxDecoderScoring.Calculate(Result("structured", 0.5, structures: [structure])), 10);
        Assert.Equal(0.5, FluxDecoderScoring.Calculate(Result("empty", 0.5)), 10);
    }

    [Fact]
    public void AutomaticSelectionBreaksTiesByConfidenceStructuresAndCatalogOrder()
    {
        var revolution = new FluxRevolution(8_000_000, []);
        var structure = new FluxStructure(FluxStructureKind.Sync, 0, 1, "sync");
        var secondStructure = new FluxStructure(FluxStructureKind.FormatData, 1, 1, "data");

        var confidenceRegistry = new FluxDecoderRegistry([new FixedDecoder("structured", Result("structured", 0, structures: [structure])), new FixedDecoder(FluxCodecIds.Raw, Result(FluxCodecIds.Raw, 1))]);
        Assert.Equal(FluxCodecIds.Raw, confidenceRegistry.DecodeAutomatic(revolution).DecoderId);

        var structuresRegistry = new FluxDecoderRegistry([new FixedDecoder("one", Result("one", 0.5, structures: [structure])), new FixedDecoder("two", Result("two", 0.5, structures: [structure, secondStructure]))]);
        Assert.Equal("two", structuresRegistry.DecodeAutomatic(revolution).DecoderId);

        var catalogRegistry = new FluxDecoderRegistry([new FixedDecoder("first", Result("first", 0.5, structures: [structure])), new FixedDecoder("second", Result("second", 0.5, structures: [structure]))]);
        Assert.Equal("first", catalogRegistry.DecodeAutomatic(revolution).DecoderId);
    }

    [Fact]
    public void DecodeBestHandlesEmptyExplicitAndAutomaticSelections()
    {
        var invalid = new DecodedSector(0, 0, 1, 0, 128, false, 0);
        var valid = invalid with { IntegrityValid = true };
        var decoder = new ConditionalDecoder("conditional", revolution => revolution.IndexTimeTicks == 1 ? Result("conditional", 0.9, sectors: [invalid]) : Result("conditional", 0.1, sectors: [valid]));
        var registry = new FluxDecoderRegistry([decoder]);
        FluxRevolution[] revolutions = [new(1, []), new(2, [])];

        Assert.Null(registry.DecodeBest([]));
        Assert.Equal(1, registry.DecodeBest(revolutions, decoder.Id)!.RevolutionIndex);
        Assert.Equal(1, registry.DecodeBest(revolutions)!.RevolutionIndex);
    }

    private static FluxDecodeResult Result(string id, double confidence, IReadOnlyList<FluxStructure>? structures = null, IReadOnlyList<DecodedSector>? sectors = null) => new(id, id, confidence, 0, structures ?? [], [], sectors);

    private sealed class StubDecoder(string id) : IFluxDecoder
    {
        public string Id { get; } = id;
        public string DisplayName => Id;
        public FluxDecodeResult Decode(FluxRevolution revolution) => new(Id, DisplayName, 0, 0, [], []);
    }

    private sealed class FixedDecoder(string id, FluxDecodeResult result) : IFluxDecoder
    {
        public string Id { get; } = id;
        public string DisplayName => Id;
        public FluxDecodeResult Decode(FluxRevolution revolution) => result;
    }

    private sealed class ConditionalDecoder(string id, Func<FluxRevolution, FluxDecodeResult> decode) : IFluxDecoder
    {
        public string Id { get; } = id;
        public string DisplayName => Id;
        public FluxDecodeResult Decode(FluxRevolution revolution) => decode(revolution);
    }

    private sealed class CountingDecoder(string id) : IFluxDecoder
    {
        public string Id { get; } = id;
        public string DisplayName => Id;
        public int ExecutionCount { get; private set; }
        public FluxDecodeResult Decode(FluxRevolution revolution)
        {
            ExecutionCount++;
            return new(Id, DisplayName, 0, 0, [], []);
        }
    }

    private sealed class BlockingDecoder(string id) : IFluxDecoder, IDisposable
    {
        private readonly ManualResetEventSlim entered = new();
        private readonly ManualResetEventSlim released = new();
        private int executionCount;
        public string Id { get; } = id;
        public string DisplayName => Id;
        public int ExecutionCount => executionCount;
        public FluxDecodeResult Decode(FluxRevolution revolution)
        {
            Interlocked.Increment(ref executionCount);
            entered.Set();
            released.Wait();
            return new(Id, DisplayName, 0, 0, [], []);
        }
        public bool WaitUntilEntered(TimeSpan timeout) => entered.Wait(timeout);
        public void Release() => released.Set();
        public void Dispose()
        {
            entered.Dispose();
            released.Dispose();
        }
    }
}
