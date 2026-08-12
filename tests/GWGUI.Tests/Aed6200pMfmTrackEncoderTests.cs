using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les en-têtes, marques, contrôles et limites de l'encodeur AED 6200P.</summary>
public sealed class Aed6200pMfmTrackEncoderTests
{
    [Theory]
    [InlineData(0x80, 0x80, 0x00)]
    [InlineData(0x1234, 0x34, 0x12)]
    public void HeaderStoresSizeLowByteThenHighByteAndValidCrc(int size, byte low, byte high)
    {
        var encoded = Encode(7, 9, new byte[size]);
        var header = DecodeHeader(encoded.Bits);

        Assert.Equal([Aed6200pMfmFormat.HeaderAddressMark, 7, low, 9, high], header.Take(5));
        Assert.Equal(0, Crc16Calculator.Compute(header));
    }

    [Theory]
    [InlineData(false, Aed6200pMfmFormat.DataMark)]
    [InlineData(true, Aed6200pMfmFormat.DeletedDataMark)]
    public void DataMarkPatternPayloadAndCrcMatchDeletionState(bool deleted, byte mark)
    {
        byte[] data = [1, 2, 3, 4];
        var encoded = Encode(2, 3, data, deleted);
        var dataOffset = Aed6200pMfmFormat.HeaderByteCount * MfmEncoding.EncodedByteBitCount + Aed6200pMfmFormat.FirstGapBitCount;
        var definition = Assert.Single(Aed6200pMfmFormat.DataMarks, candidate => candidate.Mark == mark);
        Assert.Equal(PatternBits(definition.Pattern), encoded.Bits.Skip(dataOffset).Take(definition.Pattern.Count * BitPrimitives.BitsPerByte));
        var decoded = DecodeMfm(encoded.Bits, dataOffset + definition.Pattern.Count * BitPrimitives.BitsPerByte, data.Length + Aed6200pMfmFormat.CrcByteCount);
        Assert.Equal(data, decoded.Take(data.Length));
        Assert.Equal(0, Crc16Calculator.Compute(new[] { mark }.Concat(decoded)));
    }

    [Fact]
    public void GapsAndRoundTripPreservePayloadAndIntegrity()
    {
        var data = Enumerable.Range(0, 512).Select(index => (byte)(index * 37 + 11)).ToArray();
        var encoded = Encode(2, 3, data);
        var firstGapOffset = Aed6200pMfmFormat.HeaderByteCount * MfmEncoding.EncodedByteBitCount;
        var dataBlockBitCount = (Aed6200pMfmFormat.DataMarkByteCount + data.Length + Aed6200pMfmFormat.CrcByteCount) * MfmEncoding.EncodedByteBitCount;
        var secondGapOffset = firstGapOffset + Aed6200pMfmFormat.FirstGapBitCount + dataBlockBitCount;

        Assert.Equal(Enumerable.Range(0, Aed6200pMfmFormat.FirstGapBitCount).Select(index => (index & 1) == 0), encoded.Bits.Skip(firstGapOffset).Take(Aed6200pMfmFormat.FirstGapBitCount));
        Assert.Equal(Enumerable.Range(0, Aed6200pMfmFormat.SecondGapBitCount).Select(index => (index & 1) == 0), encoded.Bits.Skip(secondGapOffset).Take(Aed6200pMfmFormat.SecondGapBitCount));
        var decoded = new Aed6200pMfmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.Equal(data, decoded.DecodedBytes.TakeLast(data.Length));
        Assert.True(sector.IntegrityValid);
    }

    [Fact]
    public void FieldLimitsAreAcceptedAndExceededValuesAreRejected()
    {
        var oneByte = new byte[1];
        new Aed6200pMfmTrackEncoder().Encode(new TrackEncodeRequest(Aed6200pMfmFormat.MaximumCylinder, 0, [new TrackSector(Aed6200pMfmFormat.MaximumSector, oneByte)]));
        new Aed6200pMfmTrackEncoder().Encode(new TrackEncodeRequest(0, 0, [new TrackSector(0, new byte[Aed6200pMfmFormat.MaximumSectorByteCount])]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Aed6200pMfmTrackEncoder().Encode(new TrackEncodeRequest(Aed6200pMfmFormat.MaximumCylinder + 1, 0, [new TrackSector(0, oneByte)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Aed6200pMfmTrackEncoder().Encode(new TrackEncodeRequest(0, 0, [new TrackSector(Aed6200pMfmFormat.MaximumSector + 1, oneByte)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Aed6200pMfmTrackEncoder().Encode(new TrackEncodeRequest(0, 0, [new TrackSector(0, new byte[Aed6200pMfmFormat.MaximumSectorByteCount + 1])])));
    }

    private static EncodedTrack Encode(int cylinder, int sector, byte[] data, bool deleted = false) => new Aed6200pMfmTrackEncoder().Encode(new TrackEncodeRequest(cylinder, 0, [new TrackSector(sector, data, deleted)]));

    private static byte[] DecodeHeader(IReadOnlyList<bool> bits) => [Aed6200pMfmFormat.HeaderAddressMark, .. DecodeMfm(bits, Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte, Aed6200pMfmFormat.HeaderByteCount - 1)];

    private static byte[] DecodeMfm(IReadOnlyList<bool> bits, int offset, int count)
    {
        var stream = new FluxBitstream(bits.ToArray(), TrackEncodingDefaults.BitCellTicks);
        var result = new byte[count];
        for (var index = 0; index < count; index++) Assert.True(FluxBitReader.TryDecodeMfmByte(stream, offset + index * MfmEncoding.EncodedByteBitCount, out result[index]));
        return result;
    }

    private static IEnumerable<bool> PatternBits(IEnumerable<byte> pattern) => pattern.SelectMany(value => Enumerable.Range(0, BitPrimitives.BitsPerByte).Select(bit => (value & (1 << (BitPrimitives.BitsPerByte - 1 - bit))) != 0));
}
