using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les adresses, blocs et checksums du format NorthStar MFM.</summary>
public sealed class NorthstarMfmDecoderTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(15, 15)]
    [InlineData(9, 6)]
    public void AddressRoundTripPreservesFourBitLimits(int cylinder, int sector)
    {
        var packed = NorthstarMfmAddress.Pack(cylinder, sector);

        Assert.Equal(((byte)cylinder, (byte)sector), NorthstarMfmAddress.Unpack(packed));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CompleteBlockReportsChecksumValidity(bool validChecksum)
    {
        var payload = Enumerable.Range(0, NorthstarMfmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var stream = BlockStream(4, 7, payload, validChecksum);
        var identity = Assert.IsType<NorthstarMfmIdentity>(NorthstarMfmDecoder.TryDecodeIdentity(stream, 0));
        var block = Assert.IsType<NorthstarMfmBlock>(NorthstarMfmDecoder.TryDecodeBlock(stream, 0, identity));

        Assert.Equal(payload, block.Data);
        Assert.Equal(validChecksum, block.ChecksumValid);
    }

    [Fact]
    public void IdentityOnlyAndTruncatedIdentityRemainIncomplete()
    {
        var bits = TrackBitEncoding.Bits();
        bits.Mfm([NorthstarMfmAddress.Pack(3, 5)]);
        var identityOnly = new FluxBitstream(bits.ToArray(), 40);
        var identity = Assert.IsType<NorthstarMfmIdentity>(NorthstarMfmDecoder.TryDecodeIdentity(identityOnly, 0));

        Assert.Null(NorthstarMfmDecoder.TryDecodeBlock(identityOnly, 0, identity));
        Assert.Null(NorthstarMfmDecoder.TryDecodeIdentity(new FluxBitstream([], 40), 0));
    }

    [Fact]
    public void MissingMarkIsNotRecognized()
    {
        var stream = new FluxBitstream(new bool[NorthstarMfmFormat.MarkBitCount], 40);

        Assert.False(FluxBitReader.MatchBytes(stream, 0, NorthstarMfmFormat.SectorMark));
    }

    [Fact]
    public void EncoderRoundTripPreservesPayloadIntegrityStructuresAndConfidence()
    {
        var payload = Enumerable.Range(0, NorthstarMfmFormat.SectorSize).Select(index => (byte)(index * 9)).ToArray();
        var encoded = new NorthstarMfmTrackEncoder().Encode(new(15, 0, [new(15, payload)]));
        var result = new NorthstarMfmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(result.Sectors);

        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.True(result.Confidence > 0);
    }

    [Fact]
    public void EncoderWritesMarkPayloadChecksumAndFinalGap()
    {
        var payload = Enumerable.Range(0, NorthstarMfmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var encoded = new NorthstarMfmTrackEncoder().Encode(new(NorthstarMfmFormat.MaximumCylinder, 0, [new(NorthstarMfmFormat.MaximumSector, payload)]));
        var stream = new FluxBitstream(encoded.Bits.ToArray(), TrackEncodingDefaults.BitCellTicks);
        var identity = Assert.IsType<NorthstarMfmIdentity>(NorthstarMfmDecoder.TryDecodeIdentity(stream, NorthstarMfmFormat.MarkBitCount));
        var block = Assert.IsType<NorthstarMfmBlock>(NorthstarMfmDecoder.TryDecodeBlock(stream, NorthstarMfmFormat.MarkBitCount, identity));

        Assert.True(FluxBitReader.MatchBytes(stream, 0, NorthstarMfmFormat.SectorMark));
        Assert.Equal((byte)NorthstarMfmFormat.MaximumCylinder, identity.Cylinder);
        Assert.Equal((byte)NorthstarMfmFormat.MaximumSector, identity.Sector);
        Assert.Equal(payload, block.Data);
        Assert.Equal(GWGUI.MediaEngine.Primitives.RotatingChecksumCalculator.Compute(payload), block.StoredChecksum);
        Assert.Equal(Enumerable.Range(0, NorthstarMfmFormat.GapBitCount).Select(index => index % 2 == 0), encoded.Bits.TakeLast(NorthstarMfmFormat.GapBitCount));
    }

    [Fact]
    public void EncoderRejectsInvalidSizeCylinderAndSector()
    {
        var encoder = new NorthstarMfmTrackEncoder();
        var payload = new byte[NorthstarMfmFormat.SectorSize];

        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new(0, payload[..^1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(NorthstarMfmFormat.MaximumCylinder + 1, 0, [new(0, payload)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new(NorthstarMfmFormat.MaximumSector + 1, payload)])));
    }

    private static FluxBitstream BlockStream(int cylinder, int sector, IReadOnlyList<byte> payload, bool validChecksum)
    {
        var checksum = GWGUI.MediaEngine.Primitives.RotatingChecksumCalculator.Compute(payload);
        if (!validChecksum) checksum ^= 1;
        var bits = TrackBitEncoding.Bits();
        bits.Mfm([NorthstarMfmAddress.Pack(cylinder, sector)]);
        bits.Mfm(payload);
        bits.Mfm([checksum]);
        return new(bits.ToArray(), 40);
    }
}
