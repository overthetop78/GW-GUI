using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie l'adresse, le checksum et les limites de l'encodeur Data General 2F.</summary>
public sealed class DataGeneralFmTrackEncoderTests
{
    /// <summary>Vérifie les deux faces, les cylindres limites et l'aller-retour public.</summary>
    [Theory]
    [InlineData(0, 0)]
    [InlineData(23, 1)]
    [InlineData(DataGeneralFmFormat.MaximumCylinder, 0)]
    public void AddressesAndPayloadRoundTrip(int cylinder, int head)
    {
        const int sectorNumber = 4;
        var payload = Enumerable.Range(0, DataGeneralFmFormat.SectorSize).Select(index => (byte)(index * 3 + 1)).ToArray();
        var encoded = new DataGeneralFmTrackEncoder().Encode(new(cylinder, head, [new TrackSector(sectorNumber, payload)]));
        var decoded = new DataGeneralFmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(head, sector.Head);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(payload, sector.Data);
        Assert.Equal(DataGeneralFmFormat.Sync, PackedPrefix(encoded.Bits, DataGeneralFmFormat.Sync.Count));
    }

    /// <summary>Vérifie l'itération terminale du checksum et l'ordre fort puis faible de ses octets.</summary>
    [Fact]
    public void ChecksumMatchesKnownTerminalIteration()
    {
        var data = Enumerable.Range(0, DataGeneralFmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var checksum = DataGeneralChecksum.Calculate(data);
        Assert.Equal((ushort)0xaaff, checksum);
        var encoded = new DataGeneralFmTrackEncoder().Encode(new(1, 0, [new TrackSector(2, data)]));
        Assert.Equal(data, Assert.Single(new DataGeneralFmDecoder().Decode(encoded.Revolution).Sectors!).Data);
    }

    /// <summary>Vérifie le rejet des tailles et champs d'adresse hors plage.</summary>
    [Fact]
    public void EncoderRejectsInvalidSizeCylinderHeadAndSector()
    {
        var encoder = new DataGeneralFmTrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[DataGeneralFmFormat.SectorSize - 1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(DataGeneralFmFormat.MaximumCylinder + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, DataGeneralFmFormat.MaximumHead + 1, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(-1, new byte[DataGeneralFmFormat.SectorSize])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(DataGeneralFmFormat.MaximumSectorNumber + 1, new byte[DataGeneralFmFormat.SectorSize])])));
    }

    private static TrackSector Sector() => new(0, new byte[DataGeneralFmFormat.SectorSize]);
    private static byte[] PackedPrefix(IReadOnlyList<bool> bits, int byteCount) => Enumerable.Range(0, byteCount).Select(index => (byte)Enumerable.Range(0, 8).Aggregate(0, (value, bit) => (value << 1) | (bits[index * 8 + bit] ? 1 : 0))).ToArray();
}
