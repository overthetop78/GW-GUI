using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les informations, parités, limites et gaps de l'encodeur Amiga MFM.</summary>
public sealed class AmigaMfmTrackEncoderTests
{
    [Fact]
    public void RemainingSectorCountDecreasesToOne()
    {
        var sectors = Enumerable.Range(0, 3).Select(number => new TrackSector(number, Payload(number))).ToArray();
        var encoded = new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(12, 1, sectors));

        for (var index = 0; index < sectors.Length; index++)
        {
            var block = DecodeSector(encoded.Bits, index);
            var info = AmigaMfmCodec.DecodeOddEven(block.Take(AmigaMfmFormat.InfoByteCount).ToArray());
            Assert.Equal([AmigaMfmFormat.FormatByte, AmigaMfmFormat.PackTrack(12, 1), (byte)index, (byte)(sectors.Length - index)], info);
        }
    }

    [Fact]
    public void EncodedHeaderDataAndParitiesRoundTripOddEven()
    {
        var payload = Payload(7);
        var encoded = new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(4, 0, [new TrackSector(6, payload)]));
        var block = DecodeSector(encoded.Bits, 0);
        var headerAndLabel = block.Take(AmigaMfmFormat.HeaderParitySourceByteCount).ToArray();
        var data = block.Skip(AmigaMfmFormat.EncodedDataOffset).Take(AmigaMfmFormat.EncodedDataByteCount).ToArray();

        Assert.Equal(new byte[AmigaMfmFormat.LabelByteCount], headerAndLabel.Skip(AmigaMfmFormat.InfoByteCount));
        Assert.Equal(payload, AmigaMfmCodec.DecodeOddEven(data));
        var headerParity = AmigaMfmCodec.CalculateParity(headerAndLabel, 0, headerAndLabel.Length);
        var dataParity = AmigaMfmCodec.CalculateSplitParity(data, 0, data.Length);
        Assert.Equal(headerParity.High, block[AmigaMfmFormat.HeaderParityHighOffset]);
        Assert.Equal(headerParity.Low, block[AmigaMfmFormat.HeaderParityLowOffset]);
        Assert.Equal(dataParity.High, block[AmigaMfmFormat.DataParityHighOffset]);
        Assert.Equal(dataParity.Low, block[AmigaMfmFormat.DataParityLowOffset]);
    }

    [Fact]
    public void TrackAndSectorLimitsAreValidatedBeforeByteConversion()
    {
        var payload = Payload(0);
        new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(AmigaMfmFormat.MaximumCylinder, AmigaMfmFormat.MaximumHead, [new TrackSector(AmigaMfmFormat.MaximumSectorNumber, payload)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(AmigaMfmFormat.MaximumCylinder + 1, 0, [new TrackSector(0, payload)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(0, AmigaMfmFormat.MaximumHead + 1, [new TrackSector(0, payload)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(0, 0, [new TrackSector(-1, payload)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(0, 0, [new TrackSector(AmigaMfmFormat.MaximumSectorNumber + 1, payload)])));
        Assert.Throws<ArgumentException>(() => new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(0, 0, [new TrackSector(0, new byte[AmigaMfmFormat.SectorByteCount - 1])])));
        var tooMany = Enumerable.Range(0, AmigaMfmFormat.MaximumRemainingSectorCount + 1).Select(number => new TrackSector(number, payload)).ToArray();
        Assert.Throws<ArgumentOutOfRangeException>(() => new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(0, 0, tooMany)));
    }

    [Fact]
    public void SynchronizationGapsAndCompleteTrackRoundTrip()
    {
        var sectors = Enumerable.Range(0, 3).Select(number => new TrackSector(number, Payload(number))).ToArray();
        var encoded = new AmigaMfmTrackEncoder().Encode(new TrackEncodeRequest(3, 1, sectors));
        Assert.Equal(Enumerable.Range(0, AmigaMfmFormat.LeadingGapBitCount).Select(index => (index & 1) == 0), encoded.Bits.Take(AmigaMfmFormat.LeadingGapBitCount));
        var sync = new[] { (byte)(AmigaMfmFormat.SyncWord >> BitPrimitives.BitsPerByte), unchecked((byte)AmigaMfmFormat.SyncWord), (byte)(AmigaMfmFormat.SyncWord >> BitPrimitives.BitsPerByte), unchecked((byte)AmigaMfmFormat.SyncWord) };
        Assert.Equal(PatternBits(sync), encoded.Bits.Skip(AmigaMfmFormat.LeadingGapBitCount).Take(AmigaMfmFormat.SyncBitCount));
        var decoded = new AmigaMfmDecoder().Decode(encoded.Revolution);
        Assert.Equal(sectors.Length, decoded.Sectors!.Count);
        foreach (var expected in sectors) Assert.Equal(expected.Data, Assert.Single(decoded.Sectors, sector => sector.Number == expected.Number).Data);
    }

    private static byte[] Payload(int seed) => Enumerable.Range(0, AmigaMfmFormat.SectorByteCount).Select(index => (byte)(seed * 23 + index * 17)).ToArray();

    private static byte[] DecodeSector(IReadOnlyList<bool> bits, int sectorIndex)
    {
        var stride = AmigaMfmFormat.LeadingGapBitCount + AmigaMfmFormat.SyncBitCount + AmigaMfmFormat.EncodedSectorByteCount * MfmEncoding.EncodedByteBitCount + AmigaMfmFormat.TrailingGapBitCount;
        var offset = sectorIndex * stride + AmigaMfmFormat.LeadingGapBitCount + AmigaMfmFormat.SyncBitCount;
        var stream = new FluxBitstream(bits.ToArray(), TrackEncodingDefaults.BitCellTicks);
        var result = new byte[AmigaMfmFormat.EncodedSectorByteCount];
        for (var index = 0; index < result.Length; index++) Assert.True(FluxBitReader.TryDecodeMfmByte(stream, offset + index * MfmEncoding.EncodedByteBitCount, out result[index]));
        return result;
    }

    private static IEnumerable<bool> PatternBits(IEnumerable<byte> pattern) => pattern.SelectMany(value => Enumerable.Range(0, BitPrimitives.BitsPerByte).Select(bit => (value & (1 << (BitPrimitives.BitsPerByte - 1 - bit))) != 0));
}
