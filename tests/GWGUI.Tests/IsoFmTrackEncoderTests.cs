using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie les marques, tailles et limites de l'encodeur ISO FM.</summary>
public sealed class IsoFmTrackEncoderTests
{
    /// <summary>Vérifie plusieurs tailles et les marques normale et supprimée par aller-retour public.</summary>
    [Theory]
    [InlineData(128, false)]
    [InlineData(256, true)]
    [InlineData(512, false)]
    public void SizesAndDataMarksRoundTrip(int size, bool deleted)
    {
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 17 + 3)).ToArray();
        var encoded = new IsoFmTrackEncoder().Encode(new(2, 1, [new TrackSector(3, data, deleted)]));
        Assert.True(Contains(encoded.Bits, Bits(IsoFmFormat.EncodedIdAddressMark)));
        Assert.True(Contains(encoded.Bits, Bits(IsoFmFormat.DataMark(deleted).Pattern)));
        var sector = Assert.Single(new IsoFmDecoder().Decode(encoded.Revolution).Sectors!);
        Assert.Equal(data, sector.Data);
        Assert.True(sector.IntegrityValid);
    }

    /// <summary>Vérifie le rejet des champs hors plage et d'un code de taille incohérent.</summary>
    [Fact]
    public void EncoderRejectsInvalidAddressAndSizeCode()
    {
        var encoder = new IsoFmTrackEncoder();
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(IsoFmFormat.MaximumAddressValue + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, TrackEncodingLimits.MaximumHead + 1, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(-1, new byte[128])])));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(1, new byte[128], SizeCode: 1)])));
    }

    private static TrackSector Sector() => new(1, new byte[128]);
    private static bool[] Bits(ushort value) => Enumerable.Range(0, 16).Select(bit => (value & (1 << (15 - bit))) != 0).ToArray();
    private static bool Contains(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern) => Enumerable.Range(0, source.Count - pattern.Count + 1).Any(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]));
}
