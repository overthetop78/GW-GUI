using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie la construction et les limites de l'encodeur Centurion MFM.</summary>
public sealed class CenturionMfmTrackEncoderTests
{
    /// <summary>Vérifie l'adresse, le remplissage, le nombre de blocs, les CRC et l'aller-retour public.</summary>
    [Theory]
    [InlineData(0, 1)]
    [InlineData(256, 1)]
    [InlineData(257, 2)]
    public void PayloadsProduceExpectedBlocksAndRoundTrip(int size, int expectedBlocks)
    {
        const byte cylinder = 12;
        const byte sectorNumber = 7;
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 31 + 9)).ToArray();
        var encoded = new CenturionMfmTrackEncoder().Encode(new(cylinder, 0, [new TrackSector(sectorNumber, data)]));
        var decoded = new CenturionMfmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(cylinder, sector.Cylinder);
        Assert.Equal(sectorNumber, sector.Number);
        Assert.Equal(expectedBlocks * CenturionMfmFormat.AllocationBlockSize, sector.Data!.Count);
        Assert.Equal(data, sector.Data.Take(size));
        Assert.All(sector.Data.Skip(size), value => Assert.Equal(CenturionMfmFormat.PaddingByte, value));
    }

    /// <summary>Vérifie le rejet des valeurs qui ne tiennent pas dans les champs d'adresse ou de taille.</summary>
    [Fact]
    public void EncoderRejectsOutOfRangeAddressAndBlockCount()
    {
        var encoder = new CenturionMfmTrackEncoder();
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(CenturionMfmFormat.MaximumAddressValue + 1, 0, [new TrackSector(0, [])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(-1, [])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(CenturionMfmFormat.MaximumAddressValue + 1, [])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[(CenturionMfmFormat.MaximumAllocationBlockCount + 1) * CenturionMfmFormat.AllocationBlockSize])])));
    }
}
