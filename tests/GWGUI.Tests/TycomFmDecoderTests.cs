using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les marques, en-têtes, données et CRC du format TYCOM FM.</summary>
public sealed class TycomFmDecoderTests
{
    [Fact]
    public void HeaderAndEveryDataMarkUseTheCommonFmPatterns()
    {
        Assert.Equal(FmAddressMarkPatterns.For(TycomFmFormat.HeaderAddressMark), TycomFmFormat.HeaderMark);
        Assert.All(TycomFmFormat.DataMarks, definition => Assert.Equal(FmAddressMarkPatterns.For(definition.Mark), definition.Pattern));
        Assert.Equal([0xf8, 0xf9, 0xfa, 0xfb], TycomFmFormat.DataMarks.Select(definition => definition.Mark));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HeaderReportsCrcValidity(bool validCrc)
    {
        var header = Assert.IsType<TycomFmHeader>(TycomFmDecoder.TryDecodeHeader(HeaderStream(8, 3, validCrc), 0));

        Assert.Equal(8, header.Cylinder);
        Assert.Equal(3, header.Sector);
        Assert.Equal(validCrc, header.CrcValid);
    }

    [Theory]
    [InlineData(0xf8, true)]
    [InlineData(0xf9, true)]
    [InlineData(0xfa, true)]
    [InlineData(0xfb, true)]
    [InlineData(0xfb, false)]
    public void DataMarkAndCrcAreDecoded(byte markValue, bool validCrc)
    {
        var payload = Enumerable.Range(0, TycomFmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var definition = TycomFmFormat.DataMarks.Single(mark => mark.Mark == markValue);
        var mark = new TycomFmDataMark(0, definition);
        var data = Assert.IsType<TycomFmData>(TycomFmDecoder.TryDecodeData(DataStream(definition, payload, validCrc), mark));

        Assert.Equal(payload, data.Payload);
        Assert.Equal(validCrc, data.CrcValid);
    }

    [Fact]
    public void NewHeaderStopsDataSearch()
    {
        var stream = HeaderStream(1, 2, true);

        Assert.Null(TycomFmDecoder.FindNextDataMark(stream, 0, stream.Bits.Length));
    }

    [Fact]
    public void MissingAndTruncatedBlocksAreRejected()
    {
        var missing = new FluxBitstream(new bool[TycomFmFormat.MaximumDataSearchDistanceBits], 40);
        var truncated = new FluxBitstream(TycomFmFormat.DataMarks[0].Pattern.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & 1 << (7 - bit)) != 0)).ToArray(), 40);

        Assert.Null(TycomFmDecoder.FindNextDataMark(missing, 0, missing.Bits.Length));
        Assert.Null(TycomFmDecoder.TryDecodeData(truncated, new TycomFmDataMark(0, TycomFmFormat.DataMarks[0])));
    }

    [Fact]
    public void UnpairedDataMarkProducesADataStructure()
    {
        var structures = new List<FluxStructure>();
        var stream = DataStream(TycomFmFormat.DataMarks[0], new byte[TycomFmFormat.SectorSize], true);

        TycomFmDecoder.CollectUnpairedDataMarks(stream, new HashSet<int>(), structures);

        Assert.Equal(FluxStructureKind.FormatData, Assert.Single(structures).Kind);
    }

    [Fact]
    public void EncoderRoundTripPreservesPayloadIntegrityStructuresAndConfidence()
    {
        var payload = Enumerable.Range(0, TycomFmFormat.SectorSize).Select(index => (byte)(index * 13)).ToArray();
        var encoded = new TycomFmTrackEncoder().Encode(new(11, 0, [new(5, payload)]));
        var result = new TycomFmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(result.Sectors);

        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
        Assert.True(result.Confidence > 0);
    }

    [Theory]
    [InlineData(false, TycomFmFormat.DataMark)]
    [InlineData(true, TycomFmFormat.DeletedDataMark)]
    public void EncoderKeepsDataMarkPatternCrcAndGapsConsistent(bool deleted, byte expectedMark)
    {
        var payload = Enumerable.Range(0, TycomFmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var encoded = new TycomFmTrackEncoder().Encode(new(8, 0, [new(3, payload, Deleted: deleted)]));
        var stream = new FluxBitstream(encoded.Bits.ToArray(), TrackEncodingDefaults.BitCellTicks);
        var header = Assert.IsType<TycomFmHeader>(TycomFmDecoder.TryDecodeHeader(stream, 0));
        var dataMark = Assert.IsType<TycomFmDataMark>(TycomFmDecoder.FindNextDataMark(stream, TycomFmFormat.HeaderBitCount, stream.Bits.Length));
        var data = Assert.IsType<TycomFmData>(TycomFmDecoder.TryDecodeData(stream, dataMark));

        Assert.Equal(8, header.Cylinder);
        Assert.Equal(3, header.Sector);
        Assert.True(header.CrcValid);
        Assert.Equal(expectedMark, dataMark.Definition.Mark);
        Assert.Equal(TycomFmFormat.SelectDataMark(deleted).Pattern, dataMark.Definition.Pattern);
        Assert.True(data.CrcValid);
        Assert.All(encoded.Bits.TakeLast(TycomFmFormat.GapBitCount), Assert.True);
    }

    [Fact]
    public void EncoderRejectsInvalidSizeCylinderAndSector()
    {
        var encoder = new TycomFmTrackEncoder();
        var payload = new byte[TycomFmFormat.SectorSize];

        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new(0, payload[..^1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(TycomFmFormat.MaximumCylinder + 1, 0, [new(0, payload)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new(TycomFmFormat.MaximumSector + 1, payload)])));
    }

    private static FluxBitstream HeaderStream(byte cylinder, byte sector, bool validCrc)
    {
        var crc = Crc16Calculator.Compute([TycomFmFormat.HeaderAddressMark, cylinder, sector], TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue);
        if (!validCrc) crc ^= 1;
        var bits = TrackBitEncoding.Bits();
        bits.Raw(TycomFmFormat.HeaderMark.ToArray());
        bits.DoubleFm([cylinder, sector, (byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]);
        return new(bits.ToArray(), 40);
    }

    private static FluxBitstream DataStream(TycomFmMarkDefinition definition, IReadOnlyList<byte> payload, bool validCrc)
    {
        var crc = Crc16Calculator.Compute(new[] { definition.Mark }.Concat(payload), TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue);
        if (!validCrc) crc ^= 1;
        var bits = TrackBitEncoding.Bits();
        bits.Raw(definition.Pattern.ToArray());
        bits.DoubleFm(payload.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        return new(bits.ToArray(), 40);
    }
}
