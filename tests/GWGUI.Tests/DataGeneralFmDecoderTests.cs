using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Vérifie l'encodage et le décodage du format Data General 2F.</summary>
public sealed class DataGeneralFmDecoderTests
{
    /// <summary>Vérifie que l'encodeur produit les deux synchronisations et un secteur décodable.</summary>
    [Fact]
    public void EncoderProducesBothSynchronizationsAndRoundTripsSector()
    {
        var payload = Payload();
        var track = new DataGeneralFmTrackEncoder().Encode(new(23, 1, [new(4, payload)]));
        var result = new DataGeneralFmDecoder().Decode(track.Revolution);

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(23, sector.Cylinder);
        Assert.Equal(1, sector.Head);
        Assert.Equal(4, sector.Number);
        Assert.Equal(DataGeneralFmFormat.SectorSizeCode, sector.SizeCode);
        Assert.Equal(DataGeneralFmFormat.SectorSize, sector.SizeBytes);
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(2, result.Structures.Count);
        Assert.True(result.Confidence > 0);
    }

    /// <summary>Vérifie les distances minimale et maximale acceptées.</summary>
    [Theory]
    [InlineData(0, true)]
    [InlineData(224, true)]
    public void HeaderDataDistanceHonorsLimits(int gapLength, bool accepted)
    {
        var result = Decode(Record(1, 0, 0, Enumerable.Repeat(byte.MaxValue, DataGeneralFmFormat.SectorSize).ToArray(), true, gapLength));

        Assert.Equal(accepted ? 1 : 0, result.Sectors.Count);
    }

    /// <summary>Vérifie les secteurs limites zéro et sept ainsi que le rejet du secteur huit.</summary>
    [Theory]
    [InlineData(0, true)]
    [InlineData(7, true)]
    [InlineData(8, false)]
    public void SectorNumberHonorsSupportedRange(byte sectorNumber, bool accepted)
    {
        var result = Decode(Record(1, 0, sectorNumber, Payload(), true, DataGeneralFmFormat.HeaderGapBitCount));

        Assert.Equal(accepted ? 1 : 0, result.Sectors.Count);
    }

    /// <summary>Vérifie les checksums valide et invalide.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ChecksumStateIsReported(bool valid)
    {
        var result = Decode(Record(1, 0, 2, Payload(), valid, DataGeneralFmFormat.HeaderGapBitCount));

        Assert.Equal(valid, Assert.Single(result.Sectors).IntegrityValid);
    }

    /// <summary>Vérifie qu'un bloc tronqué n'est pas présenté comme un secteur complet.</summary>
    [Fact]
    public void TruncatedBlockProducesNoSector()
    {
        var bits = Sync() + EncodeFm([(byte)1, (byte)0]) + new string('0', DataGeneralFmFormat.HeaderGapBitCount) + Sync() + EncodeFm(new byte[DataGeneralFmFormat.SectorSize]);

        Assert.Empty(Decode(bits).Sectors);
    }

    /// <summary>Construit une piste Data General complète.</summary>
    /// <param name="cylinder">Cylindre.</param>
    /// <param name="head">Face.</param>
    /// <param name="sector">Secteur.</param>
    /// <param name="payload">Charge utile.</param>
    /// <param name="validChecksum">Validité demandée.</param>
    /// <param name="gapLength">Distance ajoutée après l'identité.</param>
    /// <returns>Bits de la piste.</returns>
    private static string Record(byte cylinder, byte head, byte sector, IReadOnlyList<byte> payload, bool validChecksum, int gapLength)
    {
        var checksum = DataGeneralChecksum.Calculate(payload);
        if (!validChecksum) checksum ^= ushort.MaxValue;
        var identity = new[] { (byte)(cylinder | head << DataGeneralFmFormat.HeadShift), (byte)(sector << DataGeneralFmFormat.SectorShift) };
        var block = payload.Concat([(byte)(checksum >> 8), (byte)checksum]).ToArray();
        return Sync() + EncodeFm(identity) + new string('0', gapLength) + Sync() + EncodeFm(block);
    }

    /// <summary>Retourne la synchronisation encodée.</summary>
    /// <returns>Bits de synchronisation.</returns>
    private static string Sync() => EncodeFm([DataGeneralFmFormat.FirstSyncByte, DataGeneralFmFormat.SecondSyncByte]);

    /// <summary>Crée une charge utile déterministe.</summary>
    /// <returns>Charge utile de 512 octets.</returns>
    private static byte[] Payload() => Enumerable.Range(0, DataGeneralFmFormat.SectorSize).Select(index => (byte)(index * 3 + 1)).ToArray();

    /// <summary>Encode des octets en FM.</summary>
    /// <param name="values">Octets.</param>
    /// <returns>Bits encodés.</returns>
    private static string EncodeFm(IReadOnlyList<byte> values)
    {
        var result = new System.Text.StringBuilder(values.Count * DataGeneralFmFormat.EncodedByteBitCount);
        foreach (var value in values)
        {
            for (var bit = 7; bit >= 0; bit--) result.Append('1').Append((value & 1 << bit) != 0 ? '1' : '0');
        }
        return result.ToString();
    }

    /// <summary>Décode une chaîne binaire au moyen du décodeur public.</summary>
    /// <param name="bits">Bits de la piste.</param>
    /// <returns>Résultat du décodage.</returns>
    private static FluxDecodeResult Decode(string bits)
    {
        var cells = bits.Select(bit => bit == '1').ToArray();
        return new DataGeneralFmDecoder().Decode(GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(cells, 40, 8_000_000));
    }
}
