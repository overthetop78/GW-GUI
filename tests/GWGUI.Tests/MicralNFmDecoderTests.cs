using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie la marque, les champs et le checksum des blocs Micral N FM.</summary>
public sealed class MicralNFmDecoderTests
{
    [Fact]
    public void BlockDecodesSectorCylinderAndPayloadAtDefinedPositions()
    {
        var payload = Enumerable.Range(0, MicralNFmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var block = Assert.IsType<MicralNFmBlock>(MicralNFmDecoder.TryDecodeBlock(BlockStream(7, 42, payload, true), 0));

        Assert.Equal(7, block.Sector);
        Assert.Equal(42, block.Cylinder);
        Assert.Equal(payload, block.Data);
        Assert.True(block.ChecksumValid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void BlockReportsChecksumValidity(bool validChecksum)
    {
        var payload = Enumerable.Repeat((byte)0x35, MicralNFmFormat.SectorSize).ToArray();

        Assert.Equal(validChecksum, Assert.IsType<MicralNFmBlock>(MicralNFmDecoder.TryDecodeBlock(BlockStream(1, 2, payload, validChecksum), 0)).ChecksumValid);
    }

    [Fact]
    public void ChecksumHandlesUpdatesWithoutAndWithCarry()
    {
        Assert.Equal(1, MicralNChecksum.Update(0, 1));
        Assert.Equal(129, MicralNChecksum.Update(127, 1));
    }

    [Fact]
    public void MissingMarkAndTruncatedBlockAreRejected()
    {
        var missing = new FluxBitstream(new bool[MicralNFmFormat.BlockBitCount], 40);
        var truncated = new FluxBitstream(BlockStream(1, 2, new byte[MicralNFmFormat.SectorSize], true).Bits.Take(MicralNFmFormat.MarkBitCount).ToArray(), 40);

        Assert.False(FluxBitReader.MatchBytes(missing, 0, MicralNFmFormat.SectorMark));
        Assert.Null(MicralNFmDecoder.TryDecodeBlock(truncated, 0));
    }

    [Fact]
    public void EncoderRoundTripPreservesPayloadIntegrityStructuresAndConfidence()
    {
        var payload = Enumerable.Range(0, MicralNFmFormat.SectorSize).Select(index => (byte)(index * 3)).ToArray();
        var encoded = new MicralNFmTrackEncoder().Encode(new(19, 0, [new(6, payload)]));
        var result = new MicralNFmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(result.Sectors);

        Assert.Equal(19, sector.Cylinder);
        Assert.Equal(6, sector.Number);
        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.True(result.Confidence > 0);
    }

    private static FluxBitstream BlockStream(byte sector, byte cylinder, IReadOnlyList<byte> payload, bool validChecksum)
    {
        var checksum = MicralNChecksum.Compute(payload);
        if (!validChecksum) checksum ^= 1;
        var bits = TrackBitEncoding.Bits();
        bits.Raw(MicralNFmFormat.SectorMark.ToArray());
        bits.Fm(new[] { sector, cylinder }.Concat(payload).Append(checksum));
        return new(bits.ToArray(), 40);
    }
}
