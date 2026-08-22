using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

namespace GWGUI.Tests;

/// <summary>Vérifie les variantes, adresses et limites de l'encodeur DEC RX02.</summary>
public sealed class DecRx02TrackEncoderTests
{
    /// <summary>Vérifie les variantes FM et M²FM normales ou supprimées par un aller-retour public.</summary>
    [Theory]
    [InlineData(DecRx02Format.FmSectorByteCount, false)]
    [InlineData(DecRx02Format.FmSectorByteCount, true)]
    [InlineData(DecRx02Format.M2FmSectorByteCount, false)]
    [InlineData(DecRx02Format.M2FmSectorByteCount, true)]
    public void AllDataMarksRoundTrip(int size, bool deleted)
    {
        var payload = Enumerable.Range(0, size).Select(index => (byte)(index * 13 + 3)).ToArray();
        var encoded = new DecRx02TrackEncoder().Encode(new(4, 1, [new TrackSector(6, payload, Deleted: deleted)]));
        var encoding = size == DecRx02Format.M2FmSectorByteCount ? DecRx02SectorEncoding.M2Fm : DecRx02SectorEncoding.Fm;
        var mark = DecRx02Format.DataMarkFor(encoding, deleted);
        Assert.True(Contains(encoded.Bits, Bits(mark.Pattern)));
        var sector = Assert.Single(new DecRx02Decoder().Decode(encoded.Revolution).Sectors);
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
    }

    /// <summary>Vérifie le rejet des tailles, adresses et codes de taille incohérents.</summary>
    [Fact]
    public void EncoderRejectsInvalidFields()
    {
        var encoder = new DecRx02TrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[129])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(DecRx02Format.MaximumAddressValue + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(-1, new byte[DecRx02Format.FmSectorByteCount])])));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[DecRx02Format.M2FmSectorByteCount], SizeCode: DecRx02Format.FmSectorSizeCode)])));
    }

    private static TrackSector Sector() => new(0, new byte[DecRx02Format.FmSectorByteCount]);
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
    private static bool Contains(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern) => Enumerable.Range(0, source.Count - pattern.Count + 1).Any(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]));
}
