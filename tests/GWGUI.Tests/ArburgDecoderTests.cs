using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Vérifie les validations propres aux deux formes de blocs Arburg.</summary>
public sealed class ArburgDecoderTests
{
    /// <summary>Vérifie qu'un checksum FM incorrect est signalé sans perdre les données utiles.</summary>
    [Fact]
    public void InvalidFmChecksumIsReported()
    {
        var data = CreateData(ArburgFormat.DataUsefulSize);
        var block = ArburgChecksum.CreateBlock(data, ArburgFormat.DataBlockSize);
        block[ArburgFormat.DataUsefulSize] ^= 1;
        var bits = TrackEncoding.Bits();
        bits.Raw(ArburgFormat.DataMark.ToArray());
        bits.DoubleFm(block.Select(BitPrimitives.ReverseBits));

        var result = Decode(bits);

        var sector = Assert.Single(result.Sectors);
        Assert.False(sector.IntegrityValid);
        Assert.Equal(data, sector.Data);
    }

    /// <summary>Vérifie qu'un checksum système incorrect est signalé sans perdre les données utiles.</summary>
    [Fact]
    public void InvalidSystemChecksumIsReported()
    {
        var data = CreateData(ArburgFormat.SystemUsefulSize);
        var block = ArburgChecksum.CreateBlock(data, ArburgFormat.SystemBlockSize);
        block[ArburgFormat.SystemUsefulSize + 1] ^= 1;
        var bits = TrackEncoding.Bits();
        bits.Raw(ArburgFormat.SystemMark.ToArray());
        bits.AddRange(ArburgSystemCodec.Encode(block));

        var result = Decode(bits);

        var sector = Assert.Single(result.Sectors);
        Assert.False(sector.IntegrityValid);
        Assert.Equal(data, sector.Data);
    }

    /// <summary>Vérifie qu'un flux sans marque Arburg ne produit aucune structure.</summary>
    [Fact]
    public void MissingMarkProducesNoResult()
    {
        var result = Decode(Enumerable.Repeat(true, 256).ToList());

        Assert.Empty(result.Structures);
        Assert.Empty(result.Sectors);
        Assert.Equal(0, result.Confidence);
    }

    /// <summary>Vérifie qu'un bloc FM tronqué est reconnu sans données ni intégrité inventées.</summary>
    [Fact]
    public void TruncatedFmBlockHasNoPayload()
    {
        var bits = TrackEncoding.Bits();
        bits.Raw(ArburgFormat.DataMark.ToArray());
        bits.DoubleFm([0x00]);

        var result = Decode(bits);

        var sector = Assert.Single(result.Sectors);
        Assert.Null(sector.Data);
        Assert.Null(sector.IntegrityValid);
        Assert.Single(result.Structures);
    }

    /// <summary>Vérifie qu'une séquence système invalide ne produit aucune charge utile.</summary>
    [Fact]
    public void InvalidSystemSequenceHasNoPayload()
    {
        var bits = TrackEncoding.Bits();
        bits.Raw(ArburgFormat.SystemMark.ToArray());
        bits.Add(!ArburgFormat.SystemPrefixBit);
        bits.Add(ArburgFormat.SystemZeroSecondBit);

        var result = Decode(bits);

        var sector = Assert.Single(result.Sectors);
        Assert.Null(sector.Data);
        Assert.Null(sector.IntegrityValid);
        Assert.Equal(FluxStructureKind.FormatHeader, Assert.Single(result.Structures).Kind);
    }

    /// <summary>Décode les bits fournis avec la même chaîne publique que les pistes encodées.</summary>
    private static FluxDecodeResult Decode(IReadOnlyList<bool> bits) => new ArburgDecoder().Decode(TrackEncoding.ToRevolution(bits, 40, 8_000_000));

    /// <summary>Crée des données déterministes sensibles à l'ordre de leurs bits.</summary>
    private static byte[] CreateData(int size) => Enumerable.Range(0, size).Select(index => (byte)(index * 37 + 0x53)).ToArray();
}
