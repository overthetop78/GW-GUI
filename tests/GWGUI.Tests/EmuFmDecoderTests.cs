using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie le parcours public E-mu FM et ses cas incomplets.</summary>
public sealed class EmuFmDecoderTests
{
    /// <summary>Vérifie l'identité, la charge utile, les structures et la confiance d'un secteur complet.</summary>
    [Fact]
    public void CompleteSectorRoundTrips()
    {
        var payload = Payload();
        var encoded = new EmuFmTrackEncoder().Encode(new(12, 1, [new(EmuFmFormat.SectorNumber, payload)]));
        var result = new EmuFmDecoder().Decode(encoded.Revolution);

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(12, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.True(result.Confidence > 0);
        var data = Assert.Single(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
        Assert.True(data.BitOffset >= EmuFmFormat.HeaderBitCount);
    }

    /// <summary>Vérifie qu'un en-tête dont le CRC est altéré n'est pas accepté.</summary>
    [Fact]
    public void InvalidHeaderCrcIsRejected()
    {
        var encoded = new EmuFmTrackEncoder().Encode(new(12, 1, [new(EmuFmFormat.SectorNumber, Payload())]));
        var bits = encoded.Bits.ToArray();
        bits[EmuFmFormat.MarkBitCount + EmuFmFormat.EncodedFmByteBitCount + 3] = !bits[EmuFmFormat.MarkBitCount + EmuFmFormat.EncodedFmByteBitCount + 3];

        Assert.Empty(Decode(bits).Sectors);
    }

    /// <summary>Vérifie qu'une charge tronquée ne produit pas de données sectorielles.</summary>
    [Fact]
    public void TruncatedPayloadIsUnavailable()
    {
        var stream = new FluxBitstream(new bool[EmuFmFormat.MarkBitCount + EmuFmFormat.EncodedFmByteBitCount], 40);

        Assert.Null(EmuFmDecoder.TryDecodeData(stream, 0));
    }

    /// <summary>Vérifie une piste sans marque et une marque isolée non classée.</summary>
    [Fact]
    public void MissingAndUnclassifiedMarksAreDistinguished()
    {
        Assert.Empty(Decode(new bool[EmuFmFormat.MarkBitCount]).Structures);
        var markBits = EmuFmFormat.SectorMark.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & 1 << (7 - bit)) != 0)).Append(true).ToArray();
        var result = Decode(markBits);

        Assert.Empty(result.Sectors);
        Assert.Contains(result.Structures, structure => structure.Description.Contains("Unclassified", StringComparison.Ordinal));
    }

    private static byte[] Payload() => Enumerable.Range(0, EmuFmFormat.PayloadByteCount).Select(index => (byte)(index * 13)).ToArray();
    private static FluxDecodeResult Decode(IReadOnlyList<bool> bits) => new EmuFmDecoder().Decode(GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(bits, 40, 8_000_000));
}
