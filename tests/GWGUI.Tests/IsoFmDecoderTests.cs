using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions, tailles et appariements ISO FM.</summary>
public sealed class IsoFmDecoderTests
{
    [Theory]
    [InlineData(0xf57e, 0xfe, FluxStructureKind.IdAddressMark, false)]
    [InlineData(0xf56f, 0xfb, FluxStructureKind.DataAddressMark, false)]
    [InlineData(0xf56a, 0xf8, FluxStructureKind.DeletedDataAddressMark, true)]
    public void MarkDefinitionIsComplete(ushort pattern, byte mark, FluxStructureKind kind, bool deleted)
    {
        var definition = Assert.Single(IsoFmFormat.Marks, candidate => candidate.Pattern == pattern);
        Assert.Equal(mark, definition.Mark);
        Assert.Equal(kind, definition.Kind);
        Assert.Equal(deleted, definition.Deleted);
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
    public void SectorSizeCodeIsBounded(byte code, int expected) => Assert.Equal(expected, IsoFmFormat.SectorSize(code));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NormalAndDeletedSectorRoundTrip(bool deleted)
    {
        var payload = Enumerable.Range(0, 128).Select(index => (byte)(index * 7)).ToArray();
        var encoded = new IsoFmTrackEncoder().Encode(new(2, 1, [new(3, payload, deleted)]));
        var result = new IsoFmDecoder().Decode(encoded.Revolution);

        var sector = Assert.Single(result.Sectors);
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == (deleted ? FluxStructureKind.DeletedDataAddressMark : FluxStructureKind.DataAddressMark));
        Assert.Equal(IsoFmFormat.IdAddressMark, result.DecodedBytes[0]);
        Assert.Contains(deleted ? IsoFmFormat.DeletedDataAddressMark : IsoFmFormat.DataAddressMark, result.DecodedBytes);
        Assert.True(result.Confidence > 0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HeaderReportsCrc(bool valid)
    {
        byte[] fields = [2, 1, 3, 0];
        var crc = Crc16Calculator.Compute(new[] { IsoFmFormat.IdAddressMark }.Concat(fields), IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
        if (!valid) crc ^= ushort.MaxValue;
        var bits = TrackBitEncoding.Bits();
        AddMark(bits, IsoFmFormat.EncodedIdAddressMark);
        bits.Fm(fields.Concat([(byte)(crc >> 8), (byte)crc]));

        Assert.Equal(valid, IsoFmDecoder.TryDecodeHeader(new FluxBitstream(bits.ToArray(), 40), 0).CrcValid);
    }

    [Fact]
    public void TruncatedHeaderAndDataAreUnavailable()
    {
        var header = IsoFmDecoder.TryDecodeHeader(new FluxBitstream(new bool[IsoFmFormat.EncodedMarkBitCount], 40), 0);
        Assert.Null(header.CrcValid);
        var mark = new IsoFmDataMark(0, IsoFmFormat.Marks.Single(definition => definition.Mark == IsoFmFormat.DataAddressMark));
        Assert.Null(IsoFmDecoder.TryDecodeData(new FluxBitstream(new bool[IsoFmFormat.EncodedMarkBitCount], 40), mark, 128));
    }

    private static void AddMark(List<bool> bits, ushort mark) => bits.Raw((byte)(mark >> 8), (byte)mark);
}
