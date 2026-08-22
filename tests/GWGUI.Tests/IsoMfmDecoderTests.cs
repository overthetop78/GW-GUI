using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les marques, tailles, parcours circulaires et tentatives PLL ISO MFM.</summary>
public sealed class IsoMfmDecoderTests
{
    [Theory]
    [InlineData(0xfe)]
    [InlineData(0xfb)]
    [InlineData(0xf8)]
    [InlineData(0xf9)]
    public void SynchronizationReturnsFollowingMark(byte mark)
    {
        var bits = TrackBitEncoding.Bits();
        bits.RawHex(IsoMfmFormat.EncodedSyncHex);
        bits.Mfm([mark]);

        Assert.Equal(mark, IsoMfmDecoder.RecognizeMark(new FluxBitstream(bits.ToArray(), 40), 0));
    }

    [Theory]
    [InlineData(0, 128)]
    [InlineData(1, 256)]
    [InlineData(2, 512)]
    [InlineData(3, 1024)]
    [InlineData(4, 2048)]
    [InlineData(5, 4096)]
    [InlineData(6, 8192)]
    [InlineData(7, 16384)]
    [InlineData(8, 0)]
    public void SectorSizeCodeIsBounded(byte code, int size) => Assert.Equal(size, IsoMfmFormat.SectorSize(code));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NormalAndDeletedSectorRoundTrip(bool deleted)
    {
        var payload = Enumerable.Range(0, 128).Select(index => (byte)(index * 11)).ToArray();
        var encoded = new IsoMfmTrackEncoder().Encode(new(2, 1, [new(3, payload, deleted)]));
        var result = new IsoMfmDecoder().Decode(encoded.Revolution);

        Assert.Equal(payload, Assert.Single(result.Sectors).Data);
        Assert.Contains(result.Structures, structure => structure.Kind == (deleted ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
        Assert.Equal(IsoMfmFormat.IdAddressMark, result.DecodedBytes[0]);
        Assert.Contains(deleted ? IsoMfmFormat.DeletedDataAddressMark : IsoMfmFormat.DataAddressMark, result.DecodedBytes);
        Assert.True(result.EstimatedBitCellTicks > 0);
        Assert.True(result.Confidence > 0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HeaderReportsStoredCrcValidity(bool validCrc)
    {
        var fields = new byte[] { 2, 1, 3, 0 };
        var crc = Crc16Calculator.Compute(new[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.IdAddressMark }.Concat(fields));
        var bits = TrackBitEncoding.Bits();
        bits.RawHex(IsoMfmFormat.EncodedSyncHex);
        bits.Mfm(new[] { IsoMfmFormat.IdAddressMark }.Concat(fields).Concat([(byte)(crc >> 8), validCrc ? (byte)crc : (byte)(crc ^ 0xff)]));

        var header = IsoMfmDecoder.TryDecodeHeader(new FluxBitstream(bits.ToArray(), 40), 0);

        Assert.Equal(validCrc, header.CrcValid);
        Assert.Equal(128, header.Size);
        Assert.Equal(fields, header.Bytes![..IsoMfmFormat.HeaderFieldByteCount]);
    }

    [Fact]
    public void TruncatedHeaderHasNoDecodedFieldsOrCrcState()
    {
        var bits = TrackBitEncoding.Bits();
        bits.RawHex(IsoMfmFormat.EncodedSyncHex);
        bits.Mfm([IsoMfmFormat.IdAddressMark]);

        var header = IsoMfmDecoder.TryDecodeHeader(new FluxBitstream(bits.ToArray(), 40), 0);

        Assert.Null(header.CrcValid);
        Assert.Null(header.Bytes);
        Assert.Equal(0, header.Size);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    [InlineData(true, false)]
    public void DataReportsStoredCrcValidityForNormalAndDeletedMarks(bool deleted, bool validCrc)
    {
        var payload = Enumerable.Range(0, 128).Select(index => (byte)index).ToArray();
        var definition = IsoMfmFormat.Marks.Single(mark => mark.Mark == (deleted ? IsoMfmFormat.DeletedDataAddressMark : IsoMfmFormat.DataAddressMark));
        var crc = Crc16Calculator.Compute(new[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, definition.Mark }.Concat(payload));
        var bits = TrackBitEncoding.Bits();
        bits.RawHex(IsoMfmFormat.EncodedSyncHex);
        bits.Mfm(new[] { definition.Mark }.Concat(payload).Concat([(byte)(crc >> 8), validCrc ? (byte)crc : (byte)(crc ^ 0xff)]));

        var data = IsoMfmDecoder.TryDecodeData(new FluxBitstream(bits.ToArray(), 40), new IsoMfmDataMark(0, definition), payload.Length);

        Assert.NotNull(data);
        Assert.Equal(validCrc, data.CrcValid);
        Assert.Equal(payload, data.Payload);
    }

    [Fact]
    public void TruncatedDataIsRejected()
    {
        var definition = IsoMfmFormat.Marks.Single(mark => mark.Mark == IsoMfmFormat.DataAddressMark);
        var bits = TrackBitEncoding.Bits();
        bits.RawHex(IsoMfmFormat.EncodedSyncHex);
        bits.Mfm([definition.Mark, 1, 2, 3]);

        Assert.Null(IsoMfmDecoder.TryDecodeData(new FluxBitstream(bits.ToArray(), 40), new IsoMfmDataMark(0, definition), 128));
    }

    [Fact]
    public void CircularDataAtTrackStartPairsWithHeaderAtTrackEnd()
    {
        var payload = Enumerable.Range(0, 128).Select(index => (byte)index).ToArray();
        var dataCrc = Crc16Calculator.Compute(new[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.DataAddressMark }.Concat(payload));
        var headerFields = new byte[] { 1, 0, 2, 0 };
        var headerCrc = Crc16Calculator.Compute(new[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.IdAddressMark }.Concat(headerFields));
        var bits = TrackBitEncoding.Bits();
        bits.RawHex(IsoMfmFormat.EncodedSyncHex);
        bits.Mfm(new[] { IsoMfmFormat.DataAddressMark }.Concat(payload).Concat([(byte)(dataCrc >> 8), (byte)dataCrc]));
        bits.Gap(64);
        bits.RawHex(IsoMfmFormat.EncodedSyncHex);
        bits.Mfm(new[] { IsoMfmFormat.IdAddressMark }.Concat(headerFields).Concat([(byte)(headerCrc >> 8), (byte)headerCrc]));

        var result = new IsoMfmDecoder().DecodeCore(new FluxBitstream(bits.ToArray(), 40));
        Assert.Equal(payload, Assert.Single(result.Sectors).Data);
    }

    [Fact]
    public void EmptyFirstPllAttemptDoesNotStopSelection()
    {
        var calls = 0;
        var empty = Result([]);
        var valid = Result([new(0, 0, 1, 0, 128, true, 0, Data: new byte[128])]);
        var selected = IsoMfmPllSelector.Select(new FluxRevolution(8_000_000, [40, 40]), 40, _ => ++calls == 1 ? empty : valid);

        Assert.Same(valid, selected);
        Assert.Equal(IsoMfmFormat.PllFactors.Count, calls);
    }

    [Fact]
    public void FullyValidFirstPllAttemptStopsSelection()
    {
        var calls = 0;
        var valid = Result([new(0, 0, 1, 0, 128, true, 0, Data: new byte[128])]);
        var selected = IsoMfmPllSelector.Select(new FluxRevolution(8_000_000, [40, 40]), 40, _ => { calls++; return valid; });

        Assert.Same(valid, selected);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void ScorePrioritizesValidityThenDataThenSectorCount()
    {
        var valid = Result([new(0, 0, 1, 0, 128, true, 0)]);
        var data = Result([new(0, 0, 1, 0, 128, false, 0, Data: new byte[128])]);
        var count = Result([new(0, 0, 1, 0, 128, false, 0), new(0, 0, 2, 0, 128, false, 0)]);

        Assert.True(IsoMfmPllSelector.Score(valid) > IsoMfmPllSelector.Score(data));
        Assert.True(IsoMfmPllSelector.Score(data) > IsoMfmPllSelector.Score(count));
    }

    private static FluxDecodeResult Result(IReadOnlyList<DecodedSector> sectors) => new(IsoMfmFormat.CodecId, IsoMfmFormat.CodecDisplayName, 0, 40, [], [], sectors);
}
