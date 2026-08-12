using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Vérifie la reconnaissance et les rejets propres au décodeur Apple RWTS18.</summary>
public sealed class AppleRwts18DecoderTests
{
    /// <summary>Vérifie qu'une adresse complète et valide permet de reconstruire son secteur.</summary>
    [Fact]
    public void ValidAddressProducesItsSector()
    {
        var (bits, data, addressOffset) = EncodeSingleSector();

        var result = new AppleRwts18Decoder().DecodeBits(bits);

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(data, sector.Data);
        Assert.Equal(addressOffset, sector.BitOffset);
    }

    /// <summary>Vérifie le rejet de chacun des champs invalides d'une adresse RWTS18.</summary>
    /// <param name="field">Champ à rendre invalide.</param>
    [Theory]
    [InlineData("track")]
    [InlineData("sector")]
    [InlineData("checksum")]
    [InlineData("trailer")]
    public void InvalidAddressFieldIsRejected(string field)
    {
        var (bits, _, addressOffset) = EncodeSingleSector();
        var track = AppleIIGcrFormat.SixAndTwoTable[18];
        var sector = AppleIIGcrFormat.SixAndTwoTable[0];
        var checksum = AppleIIGcrFormat.SixAndTwoTable[18];
        var trailer = AppleRwts18Format.AddressTrailer;
        switch (field)
        {
            case "track": track = 0x80; break;
            case "sector": sector = AppleIIGcrFormat.SixAndTwoTable[AppleRwts18Format.SectorCount]; break;
            case "checksum": checksum = AppleIIGcrFormat.SixAndTwoTable[19]; break;
            case "trailer": trailer = 0xab; break;
        }
        WriteBytes(bits, addressOffset + AppleRwts18Format.AddressMarkBitCount, track, sector, checksum, trailer);

        var result = new AppleRwts18Decoder().DecodeBits(bits);

        Assert.Empty(result.Sectors);
    }

    /// <summary>Vérifie qu'un flux dépourvu de synchronisation ne produit aucun secteur.</summary>
    [Fact]
    public void MissingSynchronizationProducesNoSector() => Assert.Empty(new AppleRwts18Decoder().DecodeBits(new bool[256]).Sectors);

    /// <summary>Vérifie qu'un épilogue incorrect empêche l'association des données à l'adresse.</summary>
    [Fact]
    public void InvalidDataEpilogueRejectsThePayload()
    {
        var (bits, _, addressOffset) = EncodeSingleSector();
        WriteByte(bits, DataRecordOffset(addressOffset) + AppleRwts18Format.DataEpilogueOffset * 8, 0xd5);

        var sector = Assert.Single(new AppleRwts18Decoder().DecodeBits(bits).Sectors);

        Assert.Null(sector.Data);
        Assert.Null(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'un symbole étranger à la table GCR invalide la charge utile.</summary>
    [Fact]
    public void UnknownGcrSymbolRejectsThePayload()
    {
        var (bits, _, addressOffset) = EncodeSingleSector();
        WriteByte(bits, DataRecordOffset(addressOffset) + AppleRwts18Format.PayloadOffset * 8, 0x80);

        var sector = Assert.Single(new AppleRwts18Decoder().DecodeBits(bits).Sectors);

        Assert.Null(sector.Data);
        Assert.Null(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'une charge utile tronquée n'est pas présentée comme décodée.</summary>
    [Fact]
    public void TruncatedPayloadIsNotDecoded()
    {
        var (bits, _, addressOffset) = EncodeSingleSector();
        var truncated = bits.Take(DataRecordOffset(addressOffset) + AppleRwts18Format.DataEpilogueOffset * 8).ToArray();

        var sector = Assert.Single(new AppleRwts18Decoder().DecodeBits(truncated).Sectors);

        Assert.Null(sector.Data);
        Assert.Null(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'un checksum modifié conserve les données mais invalide leur intégrité.</summary>
    [Fact]
    public void InvalidDataChecksumIsReported()
    {
        var (bits, data, addressOffset) = EncodeSingleSector();
        var checksumOffset = DataRecordOffset(addressOffset) + (AppleRwts18Format.PayloadOffset + AppleRwts18Format.PayloadChecksumOffset) * 8;
        var current = ReadByte(bits, checksumOffset);
        var replacement = AppleIIGcrFormat.SixAndTwoTable.First(value => value != current);
        WriteByte(bits, checksumOffset, replacement);

        var sector = Assert.Single(new AppleRwts18Decoder().DecodeBits(bits).Sectors);

        Assert.Equal(data, sector.Data);
        Assert.False(sector.IntegrityValid);
    }

    /// <summary>Vérifie qu'un secteur franchissant la fin circulaire de la piste reste décodable.</summary>
    [Fact]
    public void SectorCrossingTrackEndIsDecoded()
    {
        var (bits, data, addressOffset) = EncodeSingleSector();
        var cut = DataRecordOffset(addressOffset) + AppleRwts18Format.PageByteCount * 8;
        var rotated = bits.Skip(cut).Concat(bits.Take(cut)).ToArray();

        var sector = Assert.Single(new AppleRwts18Decoder().DecodeBits(rotated).Sectors);

        Assert.Equal(data, sector.Data);
        Assert.True(sector.IntegrityValid);
    }

    /// <summary>Construit un secteur RWTS18 déterministe et renvoie la position de sa marque d'adresse.</summary>
    private static (bool[] Bits, byte[] Data, int AddressOffset) EncodeSingleSector()
    {
        var data = Enumerable.Range(0, AppleRwts18Format.SectorByteCount).Select(index => (byte)(index * 41 + 7)).ToArray();
        var bits = TrackBitEncoding.Bits();
        bits.Gap(AppleRwts18Format.FirstSectorGapBitCount, true);
        bits.Raw((byte)(AppleRwts18Format.EncodedAddressMark >> BitPrimitives.BitsPerByte), (byte)(AppleRwts18Format.EncodedAddressMark & byte.MaxValue), AppleIIGcrFormat.SixAndTwoTable[18], AppleIIGcrFormat.SixAndTwoTable[0], AppleIIGcrFormat.SixAndTwoTable[18], AppleRwts18Format.AddressTrailer, AppleRwts18Format.SyncByte, AppleRwts18Format.SyncByte);
        bits.Raw(AppleRwts18Format.DefaultIdentifier);
        bits.Raw(AppleRwts18Codec.EncodePayload(data));
        bits.Raw(AppleRwts18Format.DataEpilogue, AppleRwts18Format.SyncByte);
        var encodedBits = bits.ToArray();
        return (encodedBits, data, FindAddressMark(encodedBits));
    }

    /// <summary>Recherche la marque d'adresse RWTS18 dans les bits encodés.</summary>
    private static int FindAddressMark(IReadOnlyList<bool> bits)
    {
        for (var offset = 0; offset + AppleRwts18Format.AddressMarkBitCount <= bits.Count; offset++)
        {
            var value = 0;
            for (var index = 0; index < AppleRwts18Format.AddressMarkBitCount; index++) value = (value << 1) | (bits[offset + index] ? 1 : 0);
            if (value == AppleRwts18Format.EncodedAddressMark) return offset;
        }
        throw new InvalidOperationException("La marque d'adresse RWTS18 encodée est introuvable.");
    }

    /// <summary>Calcule la position du premier octet de l'enregistrement de données.</summary>
    private static int DataRecordOffset(int addressOffset) => addressOffset + AppleRwts18Format.AddressMarkBitCount + AppleRwts18Format.AddressByteCount * 8 + 2 * 8;

    /// <summary>Écrit plusieurs octets consécutifs dans une collection de bits.</summary>
    private static void WriteBytes(IList<bool> bits, int offset, params byte[] values)
    {
        foreach (var value in values)
        {
            WriteByte(bits, offset, value);
            offset += 8;
        }
    }

    /// <summary>Écrit un octet dans une collection de bits.</summary>
    private static void WriteByte(IList<bool> bits, int offset, byte value)
    {
        for (var index = 0; index < 8; index++) bits[offset + index] = (value & (1 << (7 - index))) != 0;
    }

    /// <summary>Lit un octet dans une collection de bits.</summary>
    private static byte ReadByte(IReadOnlyList<bool> bits, int offset)
    {
        byte value = 0;
        for (var index = 0; index < 8; index++) value = (byte)((value << 1) | (bits[offset + index] ? 1 : 0));
        return value;
    }
}
