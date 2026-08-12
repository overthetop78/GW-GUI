using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie la géométrie, les champs et les limites de l'encodeur Commodore GCR.</summary>
public sealed class CommodoreGcrTrackEncoderTests
{
    /// <summary>Vérifie le calcul de piste pour les deux faces et le remplacement explicite.</summary>
    [Theory]
    [InlineData(0, 0, null, 1)]
    [InlineData(0, 1, null, 36)]
    [InlineData(17, 0, null, 18)]
    [InlineData(0, 0, 25, 25)]
    public void HeaderContainsResolvedDiskTrack(int cylinder, int head, int? explicitTrack, byte expectedTrack)
    {
        var attributes = explicitTrack.HasValue ? new Dictionary<string, int> { [CommodoreGcrFormat.TrackAttributeName] = explicitTrack.Value } : null;
        var bits = new CommodoreGcrTrackEncoder().Encode(new(cylinder, head, [Sector()], attributes)).Bits;
        var header = Decode(bits, CommodoreGcrFormat.LeadingGapBitCount + CommodoreGcrFormat.RawGapBitCount + CommodoreGcrFormat.SyncGapBitCount, CommodoreGcrFormat.HeaderByteCount);
        Assert.Equal(expectedTrack, header[CommodoreGcrFormat.HeaderTrackOffset]);
        Assert.Equal(CommodoreGcrFormat.DefaultId2, header[CommodoreGcrFormat.HeaderDiskId2Offset]);
        Assert.Equal(CommodoreGcrFormat.DefaultId1, header[CommodoreGcrFormat.HeaderDiskId1Offset]);
        Assert.Equal((byte)(header[2] ^ header[3] ^ header[4] ^ header[5]), header[CommodoreGcrFormat.HeaderChecksumOffset]);
    }

    /// <summary>Vérifie les synchronisations, gaps, données, checksum et aller-retour public.</summary>
    [Fact]
    public void SectorLayoutAndDataRoundTrip()
    {
        var sector = Sector();
        var encoded = new CommodoreGcrTrackEncoder().Encode(new(3, 0, [sector]));
        Assert.All(encoded.Bits.Take(CommodoreGcrFormat.LeadingGapBitCount), Assert.True);
        Assert.All(encoded.Bits.Skip(CommodoreGcrFormat.LeadingGapBitCount).Take(CommodoreGcrFormat.RawGapBitCount), Assert.False);
        var dataOffset = CommodoreGcrFormat.LeadingGapBitCount + CommodoreGcrFormat.RawGapBitCount + CommodoreGcrFormat.SyncGapBitCount + CommodoreGcrFormat.EncodedHeaderBitCount + CommodoreGcrFormat.HeaderDataGapBitCount + CommodoreGcrFormat.SyncGapBitCount;
        var data = Decode(encoded.Bits, dataOffset, CommodoreGcrFormat.DataRecordByteCount);
        Assert.Equal(CommodoreGcrFormat.DataMark, data[0]);
        Assert.Equal(sector.Data, data.Skip(1).Take(CommodoreGcrFormat.SectorByteCount));
        Assert.Equal(CommodoreGcrChecksum.Calculate(sector.Data), data[^1]);
        Assert.Equal(sector.Data, Assert.Single(new CommodoreGcrDecoder().Decode(encoded.Revolution).Sectors!).Data);
    }

    /// <summary>Vérifie le rejet des tailles, identifiants, géométries, pistes et secteurs hors plage.</summary>
    [Fact]
    public void EncoderRejectsInvalidFields()
    {
        var encoder = new CommodoreGcrTrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[CommodoreGcrFormat.SectorByteCount - 1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(CommodoreGcrFormat.MaximumCylinder + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector()], new Dictionary<string, int> { [CommodoreGcrFormat.TrackAttributeName] = 0 })));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector()], new Dictionary<string, int> { [CommodoreGcrFormat.Id1AttributeName] = -1 })));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(CommodoreGcrFormat.MaximumByteValue + 1, new byte[CommodoreGcrFormat.SectorByteCount])])));
    }

    private static TrackSector Sector() => new(3, Enumerable.Range(0, CommodoreGcrFormat.SectorByteCount).Select(index => (byte)(index * 17 + 5)).ToArray());
    private static byte[] Decode(IReadOnlyList<bool> bits, int offset, int count) => Assert.IsType<byte[]>(CommodoreGcrCodec.TryDecodeBytes(bits, offset, count));
}
