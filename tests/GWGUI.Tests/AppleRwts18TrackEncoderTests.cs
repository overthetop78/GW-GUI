using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie la disposition, les limites et le codec de l'encodeur Apple II RWTS18.</summary>
public sealed class AppleRwts18TrackEncoderTests
{
    /// <summary>Vérifie l'identité centrale et l'identification écrite par défaut.</summary>
    [Fact]
    public void EncoderExposesCentralIdentityAndDefaultIdentifier()
    {
        var encoder = new AppleRwts18TrackEncoder();
        Assert.Equal(FluxCodecIds.AppleRwts18, encoder.Id);
        Assert.Equal(FluxCodecDisplayNames.AppleRwts18, encoder.DisplayName);
        var encoded = encoder.Encode(new(7, 0, Sectors()));
        var address = AddressPattern(7, AppleRwts18Format.LastSectorNumber);
        var firstMark = Find(encoded.Bits, address);
        Assert.Equal(AppleRwts18Format.DefaultIdentifier, ReadByte(encoded.Bits, firstMark + address.Length + 16));
    }

    /// <summary>Vérifie l'ordre décroissant, les gaps et le checksum du premier champ d'adresse.</summary>
    [Fact]
    public void TrackStartsWithLastSectorAndExpectedAddressChecksum()
    {
        const int cylinder = 7;
        var encoded = new AppleRwts18TrackEncoder().Encode(new(cylinder, 0, Sectors()));
        var firstAddress = AddressPattern(cylinder, AppleRwts18Format.LastSectorNumber);
        var firstMark = Find(encoded.Bits, firstAddress);
        Assert.Equal(AppleRwts18Format.FirstSectorGapBitCount, firstMark);
        var addressOffset = firstMark + AppleRwts18Format.AddressMarkBitCount;
        Assert.Equal(AppleIIGcrFormat.SixAndTwoTable[cylinder], ReadByte(encoded.Bits, addressOffset));
        Assert.Equal(AppleIIGcrFormat.SixAndTwoTable[AppleRwts18Format.LastSectorNumber], ReadByte(encoded.Bits, addressOffset + 8));
        Assert.Equal(AppleIIGcrFormat.SixAndTwoTable[cylinder ^ AppleRwts18Format.LastSectorNumber], ReadByte(encoded.Bits, addressOffset + 16));
        var secondMark = Find(encoded.Bits, AddressPattern(cylinder, AppleRwts18Format.LastSectorNumber - 1), firstMark + firstAddress.Length);
        Assert.Equal(AppleRwts18Format.OtherSectorGapBitCount, secondMark - FindPreviousEpilogueEnd(encoded.Bits, secondMark));
    }

    /// <summary>Vérifie la longueur et l'aller-retour du codec de charge utile.</summary>
    [Fact]
    public void PayloadCodecProduces1025SymbolsAndRoundTrips()
    {
        var data = Sector(0).Data;
        var encoded = AppleRwts18Codec.EncodePayload(data);
        Assert.Equal(AppleRwts18Format.PayloadWithChecksumSymbolCount, encoded.Length);
        var values = encoded.Select(value => AppleIIGcrFormat.InverseSixAndTwoTable[value]).ToArray();
        var decoded = AppleRwts18Codec.DecodePayload(values);
        Assert.True(decoded.Valid);
        Assert.Equal(data, decoded.Data);
        Assert.Throws<ArgumentException>(() => AppleRwts18Codec.EncodePayload(data.SkipLast(1).ToArray()));
    }

    /// <summary>Vérifie le rejet des pistes incomplètes, dupliquées et des champs hors limites.</summary>
    [Fact]
    public void EncoderRejectsInvalidTrackLayoutsAndValues()
    {
        var encoder = new AppleRwts18TrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, Sectors().SkipLast(1).ToArray())));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, Sectors().Select((sector, index) => index == 5 ? Sector(4) : sector).ToArray())));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, Sectors().Select((sector, index) => index == 5 ? Sector(6) : sector).ToArray())));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, Sectors().Select((sector, index) => index == 0 ? new TrackSector(0, new byte[AppleRwts18Format.SectorByteCount - 1]) : sector).ToArray())));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(AppleRwts18Format.MaximumCylinder + 1, 0, Sectors())));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, Sectors(), new Dictionary<string, int> { [AppleRwts18Format.IdentifierAttributeName] = -1 })));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, Sectors(), new Dictionary<string, int> { [AppleRwts18Format.IdentifierAttributeName] = AppleRwts18Format.MaximumIdentifier + 1 })));
    }

    /// <summary>Vérifie l'aller-retour public d'une piste complète.</summary>
    [Fact]
    public void PublicEncoderRoundTripsCompleteTrack()
    {
        var sectors = Sectors();
        var encoded = new FluxEncoderRegistry().Encode(FluxCodecIds.AppleRwts18, new(18, 0, sectors));
        var decoded = new FluxDecoderRegistry().Decode(FluxCodecIds.AppleRwts18, encoded.Revolution);
        Assert.Equal(AppleRwts18Format.SectorCount, decoded.Sectors!.Count);
        Assert.All(sectors, expected => Assert.Equal(expected.Data, Assert.Single(decoded.Sectors, actual => actual.Number == expected.Number).Data));
    }

    private static TrackSector[] Sectors() => Enumerable.Range(0, AppleRwts18Format.SectorCount).Select(Sector).ToArray();
    private static TrackSector Sector(int number) => new(number, Enumerable.Range(0, AppleRwts18Format.SectorByteCount).Select(index => (byte)(number * 23 + index * 41)).ToArray());
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
    private static bool[] AddressPattern(int cylinder, int sector) => Bits([(byte)(AppleRwts18Format.EncodedAddressMark >> 8), (byte)(AppleRwts18Format.EncodedAddressMark & byte.MaxValue), AppleIIGcrFormat.SixAndTwoTable[cylinder], AppleIIGcrFormat.SixAndTwoTable[sector], AppleIIGcrFormat.SixAndTwoTable[cylinder ^ sector], AppleRwts18Format.AddressTrailer]);
    private static int Find(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern, int start = 0) => Enumerable.Range(start, source.Count - pattern.Count - start + 1).First(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]));
    private static byte ReadByte(IReadOnlyList<bool> bits, int offset) => (byte)Enumerable.Range(0, 8).Aggregate(0, (value, bit) => (value << 1) | (bits[offset + bit] ? 1 : 0));
    private static int FindPreviousEpilogueEnd(IReadOnlyList<bool> bits, int before)
    {
        var pattern = Bits([AppleRwts18Format.DataEpilogue, AppleRwts18Format.SyncByte]);
        return Enumerable.Range(0, before - pattern.Length + 1).Last(offset => Enumerable.Range(0, pattern.Length).All(index => bits[offset + index] == pattern[index])) + pattern.Length;
    }
}
