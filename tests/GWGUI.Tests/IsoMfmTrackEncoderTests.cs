using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

namespace GWGUI.Tests;

/// <summary>Vérifie les synchronisations, tailles et limites de l'encodeur ISO MFM.</summary>
public sealed class IsoMfmTrackEncoderTests
{
    /// <summary>Vérifie plusieurs tailles et les marques normale et supprimée par aller-retour public.</summary>
    [Theory]
    [InlineData(128, false)]
    [InlineData(256, true)]
    [InlineData(512, false)]
    public void SizesAndMarksRoundTrip(int size, bool deleted)
    {
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 19 + 7)).ToArray();
        var encoded = new IsoMfmTrackEncoder().Encode(new(2, 1, [new TrackSector(3, data, deleted)]));
        Assert.Equal(2, Count(encoded.Bits, Bits(IsoMfmFormat.EncodedSync)));
        var sector = Assert.Single(new IsoMfmDecoder().Decode(encoded.Revolution).Sectors!, item => item.Cylinder == 2 && item.Head == 1 && item.Number == 3);
        Assert.Equal(data, sector.Data);
        Assert.True(sector.IntegrityValid);
    }

    /// <summary>Vérifie le rejet des champs hors plage et d'un code de taille incohérent.</summary>
    [Fact]
    public void EncoderRejectsInvalidAddressAndSizeCode()
    {
        var encoder = new IsoMfmTrackEncoder();
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(IsoMfmFormat.MaximumAddressValue + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, TrackEncodingLimits.MaximumHead + 1, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(-1, new byte[128])])));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(1, new byte[128], SizeCode: 1)])));
    }

    private static TrackSector Sector() => new(1, new byte[128]);
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
    private static int Count(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern) => Enumerable.Range(0, source.Count - pattern.Count + 1).Count(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]));
}
