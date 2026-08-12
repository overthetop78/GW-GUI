using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie les enregistrements et limites de l'encodeur Heathkit FM.</summary>
public sealed class HeathkitFmTrackEncoderTests
{
    /// <summary>Vérifie l'ordre volume, cylindre, secteur, le checksum inversé et l'aller-retour public.</summary>
    [Fact]
    public void IdentityAndDataRoundTripThroughCommonCodec()
    {
        byte[] identity = [7, 12, 3];
        var encodedIdentity = HeathkitFmCodec.EncodeRecord(identity);
        var decodedIdentity = HeathkitFmCodec.DecodeRecord(encodedIdentity);
        Assert.Equal(identity, decodedIdentity.Payload);
        Assert.True(decodedIdentity.Valid);
        var data = Enumerable.Range(0, HeathkitFmFormat.SectorSize).Select(index => (byte)(index * 13 + 5)).ToArray();
        var attributes = new Dictionary<string, int> { [HeathkitFmFormat.VolumeAttributeName] = identity[0] };
        var encoded = new HeathkitFmTrackEncoder().Encode(new(identity[1], 0, [new TrackSector(identity[2], data)], attributes));
        var sector = Assert.Single(new HeathkitFmDecoder().Decode(encoded.Revolution).Sectors!);
        Assert.Equal(data, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(2, Count(encoded.Bits, Bits(HeathkitFmFormat.SectorMark)));
        Assert.Equal(Enumerable.Range(0, HeathkitFmFormat.DataGapBitCount).Select(index => (index & 1) == 0), encoded.Bits.TakeLast(HeathkitFmFormat.DataGapBitCount));
    }

    /// <summary>Vérifie le rejet des tailles et adresses qui ne tiennent pas dans un octet.</summary>
    [Fact]
    public void EncoderRejectsInvalidFields()
    {
        var encoder = new HeathkitFmTrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[HeathkitFmFormat.SectorSize - 1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(HeathkitFmFormat.MaximumAddressValue + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(-1, new byte[HeathkitFmFormat.SectorSize])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector()], new Dictionary<string, int> { [HeathkitFmFormat.VolumeAttributeName] = HeathkitFmFormat.MaximumAddressValue + 1 })));
    }

    private static TrackSector Sector() => new(0, new byte[HeathkitFmFormat.SectorSize]);
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
    private static int Count(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern) => Enumerable.Range(0, source.Count - pattern.Count + 1).Count(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]));
}
