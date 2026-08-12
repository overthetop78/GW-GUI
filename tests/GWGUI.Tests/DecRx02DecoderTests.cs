using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions, le codec et l'appariement RX02.</summary>
public sealed class DecRx02DecoderTests
{
    /// <summary>Vérifie les six définitions fermées de marques RX02.</summary>
    [Theory]
    [InlineData(0xf8, "55111444", DecRx02DataEncoding.Fm, true, 128)]
    [InlineData(0xf9, "55111445", DecRx02DataEncoding.M2Fm, false, 256)]
    [InlineData(0xfa, "55111454", DecRx02DataEncoding.Fm, false, 128)]
    [InlineData(0xfb, "55111455", DecRx02DataEncoding.Fm, false, 128)]
    [InlineData(0xfc, "55111544", DecRx02DataEncoding.Fm, false, 128)]
    [InlineData(0xfd, "55111545", DecRx02DataEncoding.M2Fm, true, 256)]
    internal void DataMarkDefinitionIsComplete(byte mark, string pattern, DecRx02DataEncoding encoding, bool deleted, int size)
    {
        var definition = Assert.Single(DecRx02Format.DataMarks, candidate => candidate.Mark == mark);
        Assert.Equal(Convert.FromHexString(pattern), definition.Pattern);
        Assert.Equal(encoding, definition.Encoding);
        Assert.Equal(deleted, definition.Deleted);
        Assert.Equal(size, definition.SectorSize);
    }

    /// <summary>Vérifie les parcours complets FM et M²FM.</summary>
    [Theory]
    [InlineData(128)]
    [InlineData(256)]
    public void TrackRoundTrips(int size)
    {
        var payload = Enumerable.Range(0, size).Select(index => (byte)(index * 13 + 3)).ToArray();
        var encoded = new DecRx02TrackEncoder().Encode(new(4, 1, [new(6, payload)]));
        var decoded = new DecRx02Decoder().Decode(encoded.Revolution);

        var sector = Assert.Single(decoded.Sectors);
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.True(decoded.Confidence > 0);
        Assert.Equal(decoded.Structures.OrderBy(item => item.BitOffset), decoded.Structures);
    }

    /// <summary>Vérifie qu'un code de taille incompatible avec la marque empêche la production du secteur.</summary>
    [Fact]
    public void InconsistentHeaderSizeCodeIsRejected()
    {
        var payload = new byte[DecRx02Format.M2FmSectorByteCount];
        var encoded = new DecRx02TrackEncoder().Encode(new(4, 1, [new(6, payload, SizeCode: DecRx02Format.FmSectorSizeCode)]));

        Assert.Empty(new DecRx02Decoder().Decode(encoded.Revolution).Sectors);
    }

    /// <summary>Vérifie qu'une paire binaire M²FM invalide est rejetée.</summary>
    [Fact]
    public void InvalidM2FmRuleIsRejected()
    {
        var stream = new FluxBitstream(Enumerable.Repeat(true, DecRx02Format.EncodedMfmByteBitCount).ToArray(), 40);

        Assert.Null(DecRx02M2FmCodec.Decode(stream, 0, 1));
    }

    /// <summary>Vérifie qu'une marque isolée est conservée comme structure non appariée.</summary>
    [Fact]
    public void UnpairedDataMarkIsReported()
    {
        var definition = DecRx02Format.DataMarks.Single(mark => mark.Mark == DecRx02Format.FmDataMark);
        var bits = definition.Pattern.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & 1 << (7 - bit)) != 0)).ToArray();
        var result = new DecRx02Decoder().Decode(GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(bits, 40, 8_000_000));

        Assert.Empty(result.Sectors);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
    }

    /// <summary>Vérifie qu'un bloc de données tronqué ne fournit aucune charge utile.</summary>
    [Fact]
    public void TruncatedDataIsRejectedByCodec()
    {
        var stream = new FluxBitstream(new bool[DecRx02Format.EncodedMfmByteBitCount - 1], 40);

        Assert.Null(DecRx02M2FmCodec.Decode(stream, 0, 1));
    }
}
