using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.Tests;

/// <summary>Vérifie l'identité, la permutation et les limites de l'encodeur HP MMFM.</summary>
public sealed class HpMmfmTrackEncoderTests
{
    /// <summary>Vérifie les deux faces, la transformation des paires et l'aller-retour public.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void IdentityAndPayloadRoundTrip(int head)
    {
        const byte cylinder = 23;
        const byte sectorNumber = 4;
        Assert.Equal([BitPrimitives.ReverseBits(cylinder), BitPrimitives.ReverseBits((byte)(sectorNumber | head << HpMmfmFormat.HeadShift))], new[] { BitPrimitives.ReverseBits(cylinder), BitPrimitives.ReverseBits((byte)(sectorNumber | head << HpMmfmFormat.HeadShift)) });
        byte[] sample = [0x01, 0x82, 0x04, 0x48];
        Assert.Equal([0x41, 0x80, 0x12, 0x20], HpMmfmCodec.EncodePayload(sample));
        Assert.Equal(sample, HpMmfmCodec.DecodePayload(HpMmfmCodec.EncodePayload(sample)));
        var data = Enumerable.Range(0, HpMmfmFormat.SectorSize).Select(index => (byte)(index * 17)).ToArray();
        var sector = Assert.Single(new HpMmfmDecoder().Decode(new HpMmfmTrackEncoder().Encode(new(cylinder, head, [new TrackSector(sectorNumber, data)])).Revolution).Sectors!, item => item.Cylinder == cylinder && item.Head == head && item.Number == sectorNumber && item.IntegrityValid == true);
        Assert.Equal(data, sector.Data);
        Assert.True(sector.IntegrityValid);
    }

    /// <summary>Vérifie les synchronisations communes et le rejet des valeurs invalides.</summary>
    [Fact]
    public void EncoderUsesNamedSyncAndRejectsInvalidFields()
    {
        Assert.Equal(HpMmfmFormat.SyncPrefix, HpMmfmFormat.SectorSync.Take(HpMmfmFormat.SyncPrefix.Count));
        Assert.Equal(HpMmfmFormat.SyncPrefix, HpMmfmFormat.DataSync.Take(HpMmfmFormat.SyncPrefix.Count));
        Assert.Throws<ArgumentException>(() => HpMmfmCodec.EncodePayload([1]));
        var encoder = new HpMmfmTrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[HpMmfmFormat.SectorSize - 1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(HpMmfmFormat.MaximumCylinder + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, HpMmfmFormat.MaximumHead + 1, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(HpMmfmFormat.MaximumSector + 1, new byte[HpMmfmFormat.SectorSize])])));
    }

    private static TrackSector Sector() => new(0, new byte[HpMmfmFormat.SectorSize]);
}
