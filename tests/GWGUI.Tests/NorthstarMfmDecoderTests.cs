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
        var bits = TrackEncoding.Bits();
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

    private static FluxBitstream BlockStream(int cylinder, int sector, IReadOnlyList<byte> payload, bool validChecksum)
    {
        var checksum = TrackEncoding.RotatingChecksum(payload);
        if (!validChecksum) checksum ^= 1;
        var bits = TrackEncoding.Bits();
        bits.Mfm([NorthstarMfmAddress.Pack(cylinder, sector)]);
        bits.Mfm(payload);
        bits.Mfm([checksum]);
        return new(bits.ToArray(), 40);
    }
}
