using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie le codec GCR commun et l'appariement Commodore 900.</summary>
public sealed class Commodore900GcrDecoderTests
{
    /// <summary>Vérifie les seize symboles GCR et le rejet d'un symbole absent de la table.</summary>
    [Fact]
    public void CommonCodecRecognizesEveryNibbleAndRejectsInvalidCode()
    {
        for (byte nibble = 0; nibble < 16; nibble++)
        {
            var bits = Bits(CommodoreGcrCodec.EncodingTable[nibble], CommodoreGcrCodec.EncodedNibbleBitCount);
            Assert.True(CommodoreGcrCodec.TryDecodeNibble(bits, 0, 1, out var decoded));
            Assert.Equal(nibble, decoded);
        }
        Assert.False(CommodoreGcrCodec.TryDecodeNibble(new bool[CommodoreGcrCodec.EncodedNibbleBitCount], 0, 1, out _));
    }

    /// <summary>Vérifie que dix bits de synchronisation sont nécessaires.</summary>
    [Fact]
    public void SynchronizationMustReachMinimumLength()
    {
        var shortBits = TrackBitEncoding.Bits();
        AddRecord(shortBits, Commodore900GcrFormat.MinimumSyncBitCount - 1, Header(2, 3, true));
        var validBits = TrackBitEncoding.Bits();
        AddRecord(validBits, Commodore900GcrFormat.MinimumSyncBitCount, Header(2, 3, true));

        Assert.Empty(Decode(shortBits).Sectors);
        Assert.Single(Decode(validBits).Sectors);
    }

    /// <summary>Vérifie les checksums valides d'un en-tête et d'un secteur complet.</summary>
    [Fact]
    public void CompleteSectorIsPairedAndValid()
    {
        var payload = Enumerable.Range(0, Commodore900GcrFormat.SectorByteCount).Select(index => (byte)(index * 17 + 5)).ToArray();
        var result = Decode(Track(Header(2, 3, true), Data(payload, true)));

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.CommodoreHeader);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
        Assert.True(result.Confidence > 0);
    }

    /// <summary>Vérifie qu'un checksum d'en-tête faux invalide le secteur.</summary>
    [Fact]
    public void InvalidHeaderChecksumIsReported()
    {
        var payload = new byte[Commodore900GcrFormat.SectorByteCount];
        var result = Decode(Track(Header(2, 3, true), Data(payload, true), Header(4, 5, false), Data(payload, true)));

        Assert.False(Assert.Single(result.Sectors, sector => sector.Cylinder == 4).IntegrityValid);
    }

    /// <summary>Vérifie qu'un checksum de données faux conserve la charge utile et invalide le secteur.</summary>
    [Fact]
    public void InvalidDataChecksumIsReported()
    {
        var payload = Enumerable.Repeat((byte)0x5a, Commodore900GcrFormat.SectorByteCount).ToArray();
        var result = Decode(Track(Header(2, 3, true), Data(payload, false)));

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(payload, sector.Data);
        Assert.False(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'un en-tête sans données conserve une intégrité indéterminée.</summary>
    [Fact]
    public void HeaderWithoutDataHasUnknownIntegrity()
    {
        var result = Decode(Track(Header(2, 3, true)));

        var sector = Assert.Single(result.Sectors);
        Assert.Null(sector.Data);
        Assert.Null(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'un bloc de données sans en-tête possède sa propre structure.</summary>
    [Fact]
    public void UnpairedDataBlockHasADataStructure()
    {
        var payload = new byte[Commodore900GcrFormat.SectorByteCount];
        var result = Decode(Track(Header(2, 3, true), Data(payload, true), Data(payload, true)));

        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData && structure.Description.StartsWith("Unpaired", StringComparison.Ordinal));
    }

    /// <summary>Vérifie qu'un bloc tronqué n'est pas présenté comme un secteur décodé.</summary>
    [Fact]
    public void TruncatedBlockProducesNoSector()
    {
        var bits = TrackBitEncoding.Bits();
        AddRecord(bits, Commodore900GcrFormat.MinimumSyncBitCount, [Commodore900GcrFormat.DataMark]);

        Assert.Empty(Decode(bits).Sectors);
    }

    /// <summary>Construit une piste composée des enregistrements fournis.</summary>
    private static List<bool> Track(params byte[][] records)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var record in records)
        {
            AddRecord(bits, Commodore900GcrFormat.SyncGapBitCount, record);
            bits.Gap(Commodore900GcrFormat.RecordGapBitCount);
        }
        return bits;
    }

    /// <summary>Ajoute une synchronisation et un enregistrement encodé.</summary>
    private static void AddRecord(List<bool> bits, int syncLength, IEnumerable<byte> record)
    {
        bits.Gap(syncLength, true);
        bits.AddRange(CommodoreGcrCodec.Encode(record));
    }

    /// <summary>Construit un en-tête avec un checksum valide ou volontairement faux.</summary>
    private static byte[] Header(byte cylinder, byte sector, bool valid)
    {
        var checksum = CommodoreGcrChecksum.Calculate([Commodore900GcrFormat.HeaderMark, cylinder, sector]);
        if (!valid) checksum ^= byte.MaxValue;
        return [Commodore900GcrFormat.HeaderMark, cylinder, sector, checksum];
    }

    /// <summary>Construit un bloc de données avec un checksum valide ou volontairement faux.</summary>
    private static byte[] Data(IReadOnlyList<byte> payload, bool valid)
    {
        var checksum = CommodoreGcrChecksum.Calculate(new byte[] { Commodore900GcrFormat.DataMark }.Concat(payload));
        if (!valid) checksum ^= byte.MaxValue;
        return new byte[] { Commodore900GcrFormat.DataMark }.Concat(payload).Append(checksum).ToArray();
    }

    /// <summary>Convertit une valeur en bits de poids fort à poids faible.</summary>
    private static bool[] Bits(int value, int count) => Enumerable.Range(0, count).Select(bit => (value & (1 << (count - 1 - bit))) != 0).ToArray();

    /// <summary>Décode les bits avec la chaîne publique Commodore 900.</summary>
    private static FluxDecodeResult Decode(IReadOnlyList<bool> bits) => new Commodore900GcrDecoder().Decode(GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(bits, 40, 8_000_000));
}
