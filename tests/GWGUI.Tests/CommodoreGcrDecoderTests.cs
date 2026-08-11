using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie le décodage des pistes Commodore GCR.</summary>
public sealed class CommodoreGcrDecoderTests
{
    /// <summary>Vérifie les seize symboles GCR et le rejet d'un symbole invalide.</summary>
    [Fact]
    public void CodecRecognizesEveryNibbleAndRejectsInvalidCode()
    {
        for (byte nibble = 0; nibble < 16; nibble++)
        {
            var bits = Bits(CommodoreGcrCodec.EncodingTable[nibble], CommodoreGcrCodec.EncodedNibbleBitCount);
            Assert.True(CommodoreGcrCodec.TryDecodeNibble(bits, 0, 1, out var decoded));
            Assert.Equal(nibble, decoded);
        }
        Assert.False(CommodoreGcrCodec.TryDecodeNibble(new bool[CommodoreGcrCodec.EncodedNibbleBitCount], 0, 1, out _));
    }

    /// <summary>Vérifie un en-tête complet, ses identifiants et son checksum.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HeaderRetainsIdentityAndReportsChecksum(bool valid)
    {
        var header = Header(23, 8, 0xa1, 0x1a, valid);
        var result = Decode(Track(header));

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(23, sector.Cylinder);
        Assert.Equal(8, sector.Number);
        Assert.Equal(0xa1, result.DecodedBytes[CommodoreGcrFormat.HeaderDiskId2Offset]);
        Assert.Equal(0x1a, result.DecodedBytes[CommodoreGcrFormat.HeaderDiskId1Offset]);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader && structure.Description.Contains(valid ? "valid" : "invalid", StringComparison.Ordinal));
    }

    /// <summary>Vérifie la charge utile complète avec un checksum valide ou invalide.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DataBlockRetainsPayloadAndReportsChecksum(bool valid)
    {
        var payload = Enumerable.Range(0, CommodoreGcrFormat.SectorByteCount).Select(index => (byte)(index * 43 + 5)).ToArray();
        var result = Decode(Track(Header(23, 8, 0xa1, 0x1a, true), Data(payload, valid)));

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(payload, sector.Data);
        Assert.Equal(valid, sector.IntegrityValid);
        Assert.Equal(CommodoreGcrFormat.SectorByteCount, sector.SizeBytes);
        Assert.True(result.Confidence > 0);
    }

    /// <summary>Vérifie le rejet d'une synchronisation trop courte.</summary>
    [Fact]
    public void ShortSynchronizationIsIgnored()
    {
        var bits = TrackEncoding.Bits();
        AddRecord(bits, CommodoreGcrFormat.MinimumSyncBitCount - 1, Header(1, 2, 3, 4, true));

        Assert.Empty(Decode(bits).Sectors);
    }

    /// <summary>Vérifie qu'un bloc tronqué ne fournit aucune charge utile.</summary>
    [Fact]
    public void TruncatedDataBlockProvidesNoPayload()
    {
        var result = Decode(Track(Header(1, 2, 3, 4, true), [CommodoreGcrFormat.DataMark]));

        Assert.Null(Assert.Single(result.Sectors).Data);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.Contains("unavailable", StringComparison.Ordinal));
    }

    /// <summary>Vérifie qu'un en-tête sans données conserve une intégrité indéterminée.</summary>
    [Fact]
    public void HeaderWithoutDataHasUnknownIntegrity()
    {
        var result = Decode(Track(Header(1, 2, 3, 4, true)));

        Assert.Null(Assert.Single(result.Sectors).IntegrityValid);
    }

    /// <summary>Vérifie qu'un bloc sans en-tête ne produit aucun secteur mais reste décrit.</summary>
    [Fact]
    public void UnpairedDataBlockProducesNoSector()
    {
        var result = Decode(Track(Data(new byte[CommodoreGcrFormat.SectorByteCount], true)));

        Assert.Empty(result.Sectors);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
    }

    /// <summary>Construit une piste composée des enregistrements fournis.</summary>
    /// <param name="records">Enregistrements à encoder.</param>
    /// <returns>Bits de la piste.</returns>
    private static List<bool> Track(params byte[][] records)
    {
        var bits = TrackEncoding.Bits();
        foreach (var record in records)
        {
            AddRecord(bits, CommodoreGcrFormat.SyncGapBitCount, record);
            bits.Gap(CommodoreGcrFormat.HeaderDataGapBitCount);
        }
        return bits;
    }

    /// <summary>Ajoute une synchronisation et un enregistrement encodé.</summary>
    /// <param name="bits">Piste à compléter.</param>
    /// <param name="syncLength">Longueur de la synchronisation.</param>
    /// <param name="record">Enregistrement à encoder.</param>
    private static void AddRecord(List<bool> bits, int syncLength, IEnumerable<byte> record)
    {
        bits.Gap(syncLength, true);
        bits.AddRange(CommodoreGcrCodec.Encode(record));
    }

    /// <summary>Construit un en-tête avec le checksum demandé.</summary>
    /// <param name="track">Piste.</param>
    /// <param name="sector">Secteur.</param>
    /// <param name="id2">Second identifiant de disque.</param>
    /// <param name="id1">Premier identifiant de disque.</param>
    /// <param name="valid">Validité attendue.</param>
    /// <returns>En-tête complet.</returns>
    private static byte[] Header(byte track, byte sector, byte id2, byte id1, bool valid)
    {
        var checksum = CommodoreGcrChecksum.Calculate([sector, track, id2, id1]);
        if (!valid) checksum ^= byte.MaxValue;
        return [CommodoreGcrFormat.HeaderMark, checksum, sector, track, id2, id1];
    }

    /// <summary>Construit un bloc de données avec le checksum demandé.</summary>
    /// <param name="payload">Charge utile.</param>
    /// <param name="valid">Validité attendue.</param>
    /// <returns>Bloc complet.</returns>
    private static byte[] Data(IReadOnlyList<byte> payload, bool valid)
    {
        var checksum = CommodoreGcrChecksum.Calculate(payload);
        if (!valid) checksum ^= byte.MaxValue;
        return new byte[] { CommodoreGcrFormat.DataMark }.Concat(payload).Append(checksum).ToArray();
    }

    /// <summary>Convertit une valeur en bits de poids fort à poids faible.</summary>
    /// <param name="value">Valeur.</param>
    /// <param name="count">Nombre de bits.</param>
    /// <returns>Bits produits.</returns>
    private static bool[] Bits(int value, int count) => Enumerable.Range(0, count).Select(bit => (value & (1 << (count - 1 - bit))) != 0).ToArray();

    /// <summary>Décode les bits avec la chaîne publique Commodore GCR.</summary>
    /// <param name="bits">Bits à décoder.</param>
    /// <returns>Résultat du décodeur.</returns>
    private static FluxDecodeResult Decode(IReadOnlyList<bool> bits) => new CommodoreGcrDecoder().Decode(TrackEncoding.ToRevolution(bits, 40, 8_000_000));
}
