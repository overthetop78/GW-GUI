using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Vérifie l'adresse, les CRC et les limites de l'encodeur E-mu FM.</summary>
public sealed class EmuFmTrackEncoderTests
{
    /// <summary>Vérifie la composition inversée de la piste et l'aller-retour public.</summary>
    [Theory]
    [InlineData(0, 0, 0x00)]
    [InlineData(1, 0, 0x40)]
    [InlineData(1, 1, 0xc0)]
    [InlineData(EmuFmFormat.MaximumCylinder, 1, 0xff)]
    public void TrackAndHeadAreReversedAndRoundTrip(int cylinder, int head, byte expectedRawTrack)
    {
        Assert.Equal(expectedRawTrack, BitPrimitives.ReverseBits((byte)(cylinder << EmuFmFormat.TrackShift | head)));
        var data = Payload();
        var encoded = new EmuFmTrackEncoder().Encode(new(cylinder, head, [new TrackSector(EmuFmFormat.SectorNumber, data)]));
        var sector = Assert.Single(new EmuFmDecoder().Decode(encoded.Revolution).Sectors!);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(head, sector.Head);
        Assert.Equal(data, sector.Data);
        Assert.True(sector.IntegrityValid);
    }

    /// <summary>Vérifie des CRC connus et la répétition de la marque commune.</summary>
    [Fact]
    public void HeaderAndDataUseKnownCrcAndRepeatedMark()
    {
        Assert.Equal((ushort)0x8183, Crc16Calculator.Compute([0x40], EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue));
        Assert.Equal((ushort)0x0c1e, Crc16Calculator.Compute([0x00, 0x01, 0x02, 0x03], EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue));
        var bits = new EmuFmTrackEncoder().Encode(new(1, 0, [new TrackSector(1, Payload())])).Bits;
        Assert.Equal(2, Count(bits, Bits(EmuFmFormat.SectorMark)));
        Assert.All(bits.TakeLast(EmuFmFormat.GapBitCount), Assert.True);
    }

    /// <summary>Vérifie le rejet d'une taille, d'un cylindre et d'une face hors plage.</summary>
    [Fact]
    public void EncoderRejectsInvalidFields()
    {
        var encoder = new EmuFmTrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(1, new byte[EmuFmFormat.SectorSize - 1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(EmuFmFormat.MaximumCylinder + 1, 0, [new TrackSector(1, Payload())])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, EmuFmFormat.MaximumHead + 1, [new TrackSector(1, Payload())])));
    }

    private static byte[] Payload() => Enumerable.Range(0, EmuFmFormat.SectorSize).Select(index => (byte)(index * 11 + 3)).ToArray();
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
    private static int Count(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern) => Enumerable.Range(0, source.Count - pattern.Count + 1).Count(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]));
}
