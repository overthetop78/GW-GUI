using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Complète la vérification du format Heathkit FM.</summary>
public sealed class HeathkitFmDecoderTests
{
    /// <summary>Vérifie la marque FM commune.</summary>
    [Fact]
    public void CommonMarkContainsThreeZerosAndAddressMark()
    {
        Assert.Equal(TrackBitEncoding.EncodeFm(0, 0, 0, HeathkitFmFormat.AddressMark), HeathkitFmFormat.SectorMark);
    }

    /// <summary>Vérifie qu'un checksum d'en-tête altéré est signalé.</summary>
    [Fact]
    public void InvalidHeaderChecksumInvalidatesSector()
    {
        byte[] identity = [0, 4, 6];
        var invalidChecksum = (byte)(TrackEncoding.RotatingChecksum(identity) + 1);
        var bits = TrackEncoding.Bits();
        bits.Raw(HeathkitFmFormat.SectorMark.ToArray());
        bits.Fm(identity.Append(invalidChecksum).Select(BitPrimitives.ReverseBits));
        bits.Gap(1, true);

        Assert.False(Assert.Single(Decode(bits).Sectors).IntegrityValid);
    }

    /// <summary>Vérifie le rejet d'un bloc de données tronqué.</summary>
    [Fact]
    public void TruncatedDataIsUnavailable()
    {
        var stream = new FluxBitstream(new bool[HeathkitFmFormat.MarkBitCount + HeathkitFmFormat.EncodedFmByteBitCount], 40);

        Assert.Null(HeathkitFmDecoder.TryDecodeData(stream, 0));
    }

    /// <summary>Vérifie une piste sans marque et une marque isolée non appariée.</summary>
    [Fact]
    public void MissingAndUnpairedMarksAreDistinguished()
    {
        Assert.Empty(Decode(new bool[HeathkitFmFormat.MarkBitCount]).Structures);
        var bits = HeathkitFmFormat.SectorMark.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & 1 << (7 - bit)) != 0)).Append(true).ToArray();
        var result = Decode(bits);

        Assert.Empty(result.Sectors);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader || structure.Kind == FluxStructureKind.FormatData);
    }

    private static FluxDecodeResult Decode(IReadOnlyList<bool> bits) => new HeathkitFmDecoder().Decode(TrackEncoding.ToRevolution(bits, 40, 8_000_000));
}
