using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Reconstruction.Iso;

namespace GWGUI.Tests;

public sealed class IsoScpCandidateDecoderTests
{
    [Fact]
    public async Task ReusesDecodedCandidatesForPoliciesInspectingTheSameCapture()
    {
        var revolution = new FluxRevolution(100, [10, 10]);
        var image = new ScpImage(
            new ScpHeader(0, 0, 1, 0, 0, ScpFlags.None, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Both, 0, 0),
            [new ScpTrack(0, 0, 0, [new ScpRevolution(revolution, 2)])], true, 0);
        var reader = new CountingReader(image);
        var decoder = new CountingDecoder();
        var subject = new IsoScpCandidateDecoder(reader, new FluxDecoderRegistry([decoder]));

        var first = await subject.DecodeAsync("memory.scp", [decoder.Id], CancellationToken.None);
        var second = await subject.DecodeAsync("memory.scp", [decoder.Id], CancellationToken.None);

        Assert.Same(first, second);
        Assert.Equal(1, reader.Count);
        Assert.Equal(1, decoder.Count);
    }

    private sealed class CountingReader(ScpImage image) : IScpReader
    {
        public int Count { get; private set; }

        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            Count++;
            return Task.FromResult(image);
        }
    }

    private sealed class CountingDecoder : IFluxDecoder
    {
        public string Id => "test.iso";
        public string DisplayName => Id;
        public int Count { get; private set; }

        public FluxDecodeResult Decode(FluxRevolution revolution)
        {
            Count++;
            var sector = new DecodedSector(0, 0, 1, 0, 1, true, 0, Data: [42]);
            return new(Id, DisplayName, 1, 0, [], [], [sector]);
        }
    }
}
