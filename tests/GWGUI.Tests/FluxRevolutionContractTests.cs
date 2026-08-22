using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

public sealed class FluxRevolutionContractTests
{
    [Fact]
    public void FluxRevolutionCopiesAndProtectsIntervals()
    {
        List<uint> source = [40, 80];
        var revolution = new FluxRevolution(1_000, source);
        source[0] = 120;

        Assert.Equal([40u, 80u], revolution.FluxIntervals);
        var exposed = Assert.IsAssignableFrom<IList<uint>>(revolution.FluxIntervals);
        Assert.True(exposed.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => exposed.Add(160));
    }

    [Fact]
    public void ScpRevolutionExposesSameGenericDurationAndIntervals()
    {
        var revolution = new ScpRevolution(8_000_000, 2, [40, 80]);

        Assert.Equal(revolution.IndexTimeTicks, revolution.Flux.IndexTimeTicks);
        Assert.Same(revolution.FluxIntervals, revolution.Flux.FluxIntervals);
    }

    [Fact]
    public void DecoderConsumesGenericFluxRevolution()
    {
        var result = new RawFluxDecoder().Decode(new FluxRevolution(1_000, [40, 80]));

        Assert.Equal("raw", result.DecoderId);
    }

    [Fact]
    public void EmptyRevolutionAndEncoderDecoderRoundTripUseGenericModel()
    {
        Assert.Empty(new RawFluxDecoder().Decode(new FluxRevolution(0, [])).Sectors);

        var data = Enumerable.Range(0, 128).Select(index => (byte)index).ToArray();
        var encoded = new FluxEncoderRegistry().Encode("iso.fm", new TrackEncodeRequest(0, 0, [new TrackSector(1, data)]));
        var decoded = new FluxDecoderRegistry().Decode("iso.fm", encoded.Revolution);

        Assert.IsType<FluxRevolution>(encoded.Revolution);
        Assert.Equal(data, Assert.Single(decoded.Sectors).Data);
    }
}
