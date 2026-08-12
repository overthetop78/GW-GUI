using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie les champs et limites de l'encodeur GCR Commodore 900.</summary>
public sealed class Commodore900GcrTrackEncoderTests
{
    /// <summary>Vérifie les seize codes GCR, leur ordre de bits et leur décodage.</summary>
    [Fact]
    public void CommonCodecEncodesAllNibblesMostSignificantBitFirst()
    {
        var values = Enumerable.Range(0, 16).Select(value => (byte)((value << 4) | value)).ToArray();
        var bits = CommodoreGcrCodec.Encode(values);
        Assert.Equal(values.Length * CommodoreGcrCodec.EncodedByteBitCount, bits.Count);
        for (var index = 0; index < values.Length; index++)
        {
            Assert.True(CommodoreGcrCodec.TryDecodeByte(bits, index * CommodoreGcrCodec.EncodedByteBitCount, out var decoded));
            Assert.Equal(values[index], decoded);
        }
    }

    /// <summary>Vérifie synchronisations, gaps, en-tête, données, checksums et aller-retour public.</summary>
    [Fact]
    public void CompleteSectorProducesNamedRecordsAndRoundTrips()
    {
        const byte cylinder = 4;
        const byte sectorNumber = 6;
        var payload = Enumerable.Range(0, Commodore900GcrFormat.SectorByteCount).Select(index => (byte)(index * 17 + 5)).ToArray();
        var encoded = new Commodore900GcrTrackEncoder().Encode(new(cylinder, 0, [new TrackSector(sectorNumber, payload)]));
        Assert.All(encoded.Bits.Take(Commodore900GcrFormat.SyncGapBitCount), Assert.True);
        var headerOffset = Commodore900GcrFormat.SyncGapBitCount;
        var header = Decode(encoded.Bits, headerOffset, Commodore900GcrFormat.HeaderByteCount);
        Assert.Equal([Commodore900GcrFormat.HeaderMark, cylinder, sectorNumber, CommodoreGcrChecksum.Calculate([Commodore900GcrFormat.HeaderMark, cylinder, sectorNumber])], header);
        var secondSync = headerOffset + Commodore900GcrFormat.EncodedHeaderBitCount + Commodore900GcrFormat.RecordGapBitCount;
        Assert.All(encoded.Bits.Skip(secondSync).Take(Commodore900GcrFormat.SyncGapBitCount), Assert.True);
        var data = Decode(encoded.Bits, secondSync + Commodore900GcrFormat.SyncGapBitCount, Commodore900GcrFormat.DataRecordByteCount);
        Assert.Equal(Commodore900GcrFormat.DataMark, data[0]);
        Assert.Equal(payload, data.Skip(1).Take(payload.Length));
        Assert.Equal(CommodoreGcrChecksum.Calculate(data.SkipLast(1)), data[^1]);
        var decoded = new Commodore900GcrDecoder().Decode(encoded.Revolution);
        Assert.Equal(payload, Assert.Single(decoded.Sectors!).Data);
    }

    /// <summary>Vérifie le rejet des tailles et adresses non représentables.</summary>
    [Fact]
    public void EncoderRejectsInvalidSizeAndAddresses()
    {
        var encoder = new Commodore900GcrTrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[Commodore900GcrFormat.SectorByteCount - 1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(Commodore900GcrFormat.MaximumAddressValue + 1, 0, [new TrackSector(0, new byte[Commodore900GcrFormat.SectorByteCount])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(-1, new byte[Commodore900GcrFormat.SectorByteCount])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(Commodore900GcrFormat.MaximumAddressValue + 1, new byte[Commodore900GcrFormat.SectorByteCount])])));
    }

    private static byte[] Decode(IReadOnlyList<bool> bits, int offset, int count) => Assert.IsType<byte[]>(CommodoreGcrCodec.TryDecodeBytes(bits, offset, count));
}
