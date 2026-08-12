using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie le GCR entrelacé, les en-têtes et les checksums Victor 9000.</summary>
public sealed class Victor9kGcrDecoderTests
{
    [Fact]
    public void EveryGcrSymbolDecodesWithStrideTwoAndInvalidSymbolFails()
    {
        for (byte value = 0; value < CommodoreGcrCodec.EncodingTable.Count; value++)
        {
            var bits = new bool[CommodoreGcrCodec.EncodedNibbleBitCount * Victor9kGcrFormat.EncodedCellStride];
            var code = CommodoreGcrCodec.EncodingTable[value];
            for (var bit = 0; bit < CommodoreGcrCodec.EncodedNibbleBitCount; bit++) bits[bit * Victor9kGcrFormat.EncodedCellStride] = (code & 1 << (CommodoreGcrCodec.EncodedNibbleBitCount - 1 - bit)) != 0;
            Assert.True(CommodoreGcrCodec.TryDecodeNibble(bits, 0, Victor9kGcrFormat.EncodedCellStride, out var decoded));
            Assert.Equal(value, decoded);
        }
        Assert.False(CommodoreGcrCodec.TryDecodeNibble(new bool[CommodoreGcrCodec.EncodedNibbleBitCount * Victor9kGcrFormat.EncodedCellStride], 0, Victor9kGcrFormat.EncodedCellStride, out _));
    }

    [Fact]
    public void HeaderAndDataMarksUseInterleavedDataStartingAtBitFortyNine()
    {
        var header = HeaderBlock([Victor9kGcrFormat.HeaderType, 3, 4, 7, Victor9kGcrFormat.HeaderId2, Victor9kGcrFormat.HeaderId1]);
        var data = DataBlock(new byte[Victor9kGcrFormat.SectorByteCount], true);

        Assert.True(FluxBitReader.MatchBytes(header, 0, Victor9kGcrFormat.HeaderMark));
        Assert.True(FluxBitReader.MatchBytes(data, 0, Victor9kGcrFormat.DataMark));
        Assert.Equal(3, Assert.IsType<Victor9kHeader>(Victor9kGcrDecoder.TryDecodeHeader(header.Bits, 0)).Cylinder);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void HeaderRejectsEachInvalidFixedOrSumByte(int index)
    {
        byte[] bytes = [Victor9kGcrFormat.HeaderType, 3, 4, 7, Victor9kGcrFormat.HeaderId2, Victor9kGcrFormat.HeaderId1];
        bytes[index] ^= 1;

        Assert.False(Assert.IsType<Victor9kHeader>(Victor9kGcrDecoder.TryDecodeHeader(HeaderBlock(bytes).Bits, 0)).Valid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DataChecksumUsesLowThenHighOrder(bool validChecksum)
    {
        var payload = Enumerable.Range(0, Victor9kGcrFormat.SectorByteCount).Select(index => (byte)index).ToArray();
        var stream = DataBlock(payload, validChecksum);
        var data = Assert.IsType<Victor9kData>(Victor9kGcrDecoder.TryDecodeData(stream.Bits, 0));
        var decoded = CommodoreGcrCodec.TryDecodeBytes(stream.Bits, Victor9kGcrFormat.EncodedDataStartBitOffset, Victor9kGcrFormat.DecodedDataByteCount, Victor9kGcrFormat.EncodedCellStride, out _)!;

        Assert.Equal(validChecksum, data.ChecksumValid);
        Assert.Equal((byte)data.StoredChecksum, decoded[Victor9kGcrFormat.ChecksumLowOffset]);
        Assert.Equal((byte)(data.StoredChecksum >> 8), decoded[Victor9kGcrFormat.ChecksumHighOffset]);
    }

    [Fact]
    public void TruncatedAndMissingBlocksAreRejectedAndUnpairedDataIsReported()
    {
        var truncated = new FluxBitstream(Victor9kGcrFormat.DataMark.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & 1 << (7 - bit)) != 0)).ToArray(), 40);
        var missing = new FluxBitstream(new bool[Victor9kGcrFormat.MaximumDataSearchDistanceBits], 40);
        var structures = new List<FluxStructure>();

        Assert.Null(Victor9kGcrDecoder.TryDecodeData(truncated.Bits, 0));
        Assert.Equal(-1, Victor9kGcrDecoder.FindDataMark(missing, 0, missing.Bits.Length));
        Victor9kGcrDecoder.CollectUnpairedDataMarks(DataBlock(new byte[Victor9kGcrFormat.SectorByteCount], true), new HashSet<int>(), structures);
        Assert.Equal(FluxStructureKind.FormatData, Assert.Single(structures).Kind);
    }

    [Fact]
    public void EncoderRoundTripPreservesPayloadIntegrityStructuresAndConfidence()
    {
        var payload = Enumerable.Range(0, Victor9kGcrFormat.SectorByteCount).Select(index => (byte)(index * 7)).ToArray();
        var encoded = new Victor9kGcrTrackEncoder().Encode(new(6, 0, [new(8, payload)]));
        var result = new Victor9kGcrDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(result.Sectors);

        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
        Assert.True(result.Confidence > 0);
    }

    private static FluxBitstream HeaderBlock(IEnumerable<byte> values) => Block(Victor9kGcrFormat.HeaderMark, values);

    private static FluxBitstream DataBlock(IReadOnlyList<byte> payload, bool validChecksum)
    {
        var checksum = Victor9kChecksum.Compute(payload);
        if (!validChecksum) checksum ^= 1;
        return Block(Victor9kGcrFormat.DataMark, new[] { Victor9kGcrFormat.DataPrefix }.Concat(payload).Concat([(byte)checksum, (byte)(checksum >> 8)]));
    }

    private static FluxBitstream Block(IReadOnlyList<byte> marker, IEnumerable<byte> values)
    {
        var bits = TrackBitEncoding.Bits();
        bits.Raw(marker.ToArray());
        CommodoreGcrCodec.Write(bits, Victor9kGcrFormat.EncodedDataStartBitOffset, values, Victor9kGcrFormat.EncodedCellStride);
        return new(bits.ToArray(), 40);
    }
}
