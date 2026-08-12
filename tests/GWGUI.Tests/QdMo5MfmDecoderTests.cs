using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les motifs, champs, préfixes et checksums du format QD MO5 MFM.</summary>
public sealed class QdMo5MfmDecoderTests
{
    [Fact]
    public void HeaderAndDataPatternsShareTheTenBytePreamble()
    {
        Assert.Equal(QdMo5MfmFormat.Preamble, QdMo5MfmFormat.HeaderMark.Take(QdMo5MfmFormat.PreambleByteCount));
        Assert.Equal(QdMo5MfmFormat.Preamble, QdMo5MfmFormat.DataMark.Take(QdMo5MfmFormat.PreambleByteCount));
        Assert.Equal(QdMo5MfmFormat.EncodedHeaderMark, QdMo5MfmFormat.HeaderMark.Skip(QdMo5MfmFormat.PreambleByteCount));
        Assert.Equal(QdMo5MfmFormat.EncodedDefaultDataPrefix, QdMo5MfmFormat.DataMark.Skip(QdMo5MfmFormat.PreambleByteCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65535)]
    public void HeaderDecodesWideSectorNumberAndReservedBytes(int sector)
    {
        var header = Assert.IsType<QdMo5MfmHeader>(QdMo5MfmDecoder.TryDecodeHeader(HeaderStream(sector), 0));

        Assert.Equal(sector, header.Sector);
        Assert.Equal(QdMo5MfmFormat.HeaderPaddingByteCount, header.ReservedBytes.Length);
        Assert.All(header.ReservedBytes, value => Assert.Equal(0, value));
    }

    [Theory]
    [InlineData(0x5a)]
    [InlineData(0x33)]
    public void EncoderUsesDefaultOrExplicitPrefix(int prefix)
    {
        var payload = new byte[QdMo5MfmFormat.SectorSize];
        var attributes = prefix == QdMo5MfmFormat.DefaultPrefix ? null : new Dictionary<string, int> { [QdMo5MfmFormat.PrefixAttribute] = prefix };
        var encoded = new QdMo5MfmTrackEncoder().Encode(new(0, 0, [new(1, payload, Attributes: attributes)]));
        var stream = new FluxBitstream(encoded.Bits.ToArray(), 40);
        var dataOffset = QdMo5MfmDecoder.FindNextData(stream, QdMo5MfmFormat.HeaderBitCount, stream.Bits.Length);
        var data = Assert.IsType<QdMo5MfmData>(QdMo5MfmDecoder.TryDecodeData(stream, dataOffset));

        Assert.Equal(prefix, data.Prefix);
        Assert.True(data.ChecksumValid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CompleteDataReportsChecksumValidity(bool validChecksum)
    {
        var payload = Enumerable.Range(0, QdMo5MfmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var data = Assert.IsType<QdMo5MfmData>(QdMo5MfmDecoder.TryDecodeData(DataStream(0x42, payload, validChecksum), 0));

        Assert.Equal(payload, data.Payload);
        Assert.Equal(validChecksum, data.ChecksumValid);
    }

    [Fact]
    public void TruncatedHeaderAndDataAreRejected()
    {
        var header = HeaderStream(1);
        var data = DataStream(QdMo5MfmFormat.DefaultPrefix, new byte[QdMo5MfmFormat.SectorSize], true);

        Assert.Null(QdMo5MfmDecoder.TryDecodeHeader(new FluxBitstream(header.Bits.Take(QdMo5MfmFormat.PhysicalMarkBitCount).ToArray(), 40), 0));
        Assert.Null(QdMo5MfmDecoder.TryDecodeData(new FluxBitstream(data.Bits.Take(QdMo5MfmFormat.PhysicalMarkBitCount).ToArray(), 40), 0));
    }

    [Fact]
    public void MissingMarkAndNewHeaderStopDataSearch()
    {
        var missing = new FluxBitstream(new bool[QdMo5MfmFormat.DataSearchBitCount], 40);
        var header = HeaderStream(1);

        Assert.Equal(-1, QdMo5MfmDecoder.FindNextData(missing, 0, missing.Bits.Length));
        Assert.Equal(-1, QdMo5MfmDecoder.FindNextData(header, 0, header.Bits.Length));
    }

    [Fact]
    public void UnpairedDataMarkProducesADataStructure()
    {
        var structures = new List<FluxStructure>();
        var stream = DataStream(QdMo5MfmFormat.DefaultPrefix, new byte[QdMo5MfmFormat.SectorSize], true);

        QdMo5MfmDecoder.CollectUnpairedDataMarks(stream, new HashSet<int>(), structures);

        Assert.Equal(FluxStructureKind.FormatData, Assert.Single(structures).Kind);
    }

    [Fact]
    public void EncoderRoundTripPreservesPayloadIntegrityStructuresAndConfidence()
    {
        var payload = Enumerable.Range(0, QdMo5MfmFormat.SectorSize).Select(index => (byte)(index * 11)).ToArray();
        var encoded = new QdMo5MfmTrackEncoder().Encode(new(0, 0, [new(513, payload)]));
        var result = new QdMo5MfmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(result.Sectors);

        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
        Assert.True(result.Confidence > 0);
    }

    private static FluxBitstream HeaderStream(int sector)
    {
        var bits = TrackBitEncoding.Bits();
        bits.Raw(QdMo5MfmFormat.Preamble.ToArray());
        bits.Mfm([QdMo5MfmFormat.HeaderAddressMark]);
        bits.Mfm([(byte)(sector >> 8), (byte)sector]);
        bits.Mfm(new byte[QdMo5MfmFormat.HeaderPaddingByteCount]);
        return new(bits.ToArray(), 40);
    }

    private static FluxBitstream DataStream(byte prefix, IReadOnlyList<byte> payload, bool validChecksum)
    {
        var checksum = QdMo5Checksum.Compute(prefix, payload);
        if (!validChecksum) checksum ^= 1;
        var bits = TrackBitEncoding.Bits();
        bits.Raw(QdMo5MfmFormat.Preamble.ToArray());
        bits.Mfm(new[] { prefix }.Concat(payload).Append(checksum));
        return new(bits.ToArray(), 40);
    }
}
