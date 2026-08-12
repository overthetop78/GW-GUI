using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les records, positions et checksums du format Micropolis MFM.</summary>
public sealed class MicropolisMfmDecoderTests
{
    [Fact]
    public void RecordModelPlacesEveryFieldAtItsDefinedPosition()
    {
        var payload = Enumerable.Range(0, MicropolisMfmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var record = MicropolisMfmRecord.Create(17, 6, payload);

        Assert.Equal(MicropolisMfmFormat.RecordByteCount, record.Bytes.Length);
        Assert.Equal(MicropolisMfmFormat.AddressMark, record.Bytes[MicropolisMfmFormat.AddressMarkOffset]);
        Assert.Equal(17, record.Bytes[MicropolisMfmFormat.CylinderOffset]);
        Assert.Equal(6, record.Bytes[MicropolisMfmFormat.SectorOffset]);
        Assert.Equal(payload, record.Bytes.AsSpan(MicropolisMfmFormat.DataOffset, MicropolisMfmFormat.SectorSize).ToArray());
        Assert.Equal(record.StoredChecksum, record.Bytes[MicropolisMfmFormat.ChecksumOffset]);
        Assert.All(record.Trailer, value => Assert.Equal(0, value));
        Assert.True(record.ChecksumValid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CompleteRecordReportsChecksumValidity(bool validChecksum)
    {
        var record = MicropolisMfmRecord.Create(2, 3, new byte[MicropolisMfmFormat.SectorSize]);
        if (!validChecksum) record.Bytes[MicropolisMfmFormat.ChecksumOffset] ^= 1;
        var bits = TrackEncoding.Bits();
        bits.Mfm(record.Bytes);

        var decoded = Assert.IsType<MicropolisMfmRecord>(MicropolisMfmDecoder.TryDecodeRecord(new FluxBitstream(bits.ToArray(), 40), 0));

        Assert.Equal(validChecksum, decoded.ChecksumValid);
    }

    [Fact]
    public void TruncatedRecordAndMissingSynchronizationAreRejected()
    {
        var truncated = new FluxBitstream(new bool[(MicropolisMfmFormat.RecordByteCount - 1) * MicropolisMfmFormat.EncodedByteBitCount], 40);
        var missingSync = new FluxBitstream(new bool[MicropolisMfmFormat.SyncBitCount], 40);

        Assert.Null(MicropolisMfmDecoder.TryDecodeRecord(truncated, 0));
        Assert.False(FluxBitReader.MatchBytes(missingSync, 0, MicropolisMfmFormat.Sync));
    }

    [Fact]
    public void EncoderWritesFortyZeroBytesAndRoundTripsPayload()
    {
        var payload = Enumerable.Range(0, MicropolisMfmFormat.SectorSize).Select(index => (byte)(index * 5)).ToArray();
        var encoded = new MicropolisMfmTrackEncoder().Encode(new(12, 0, [new(4, payload)]));
        var preamble = encoded.Bits.Take(MicropolisMfmFormat.PreambleByteCount * MicropolisMfmFormat.EncodedByteBitCount).ToArray();
        var result = new MicropolisMfmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(result.Sectors);

        Assert.All(preamble.Select((bit, index) => (bit, index)), item => Assert.Equal(item.index % 2 == 0, item.bit));
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.True(result.Confidence > 0);
    }
}
