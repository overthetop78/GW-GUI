using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les adresses, marques, CRC et blocs incomplets du format Membrain MFM.</summary>
public sealed class MembrainMfmDecoderTests
{
    /// <summary>Vérifie que l'encodeur rejette les tailles et champs qui ne tiennent pas dans l'adresse Membrain.</summary>
    [Fact]
    public void EncoderRejectsInvalidSizeAndAddressFields()
    {
        var encoder = new MembrainMfmTrackEncoder();
        var data = new byte[MembrainMfmFormat.SectorSize];
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, data.SkipLast(1).ToArray())])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(MembrainMfmFormat.MaximumCylinder + 1, 0, [new TrackSector(0, data)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, MembrainMfmFormat.MaximumHead + 1, [new TrackSector(0, data)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [new TrackSector(MembrainMfmFormat.MaximumSector + 1, data)])));
    }
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(255, 1, 15)]
    [InlineData(37, 1, 9)]
    public void AddressRoundTripPreservesAllFields(int cylinder, int head, int sector)
    {
        var packed = MembrainMfmAddress.Pack(cylinder, head, sector);

        Assert.Equal(((byte)cylinder, (byte)head, (byte)sector), MembrainMfmAddress.Unpack(packed.CylinderHigh, packed.PackedAddress));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HeaderReportsCrcValidity(bool validCrc)
    {
        var stream = HeaderStream(37, 1, 9, validCrc);

        var header = Assert.IsType<MembrainMfmHeader>(MembrainMfmDecoder.TryDecodeHeader(stream, 0));

        Assert.Equal(37, header.Cylinder);
        Assert.Equal(1, header.Head);
        Assert.Equal(9, header.Sector);
        Assert.Equal(validCrc, header.CrcValid);
    }

    [Theory]
    [InlineData(0xf8)]
    [InlineData(0xf9)]
    [InlineData(0xfa)]
    [InlineData(0xfb)]
    public void EveryAcceptedDataMarkDecodes(byte mark)
    {
        var payload = Enumerable.Range(0, MembrainMfmFormat.SectorSize).Select(index => (byte)index).ToArray();
        var data = Assert.IsType<MembrainMfmData>(MembrainMfmDecoder.TryDecodeData(DataStream(mark, payload, true), 0));

        Assert.Equal(mark, data.Mark);
        Assert.Equal(payload, data.Payload);
        Assert.True(data.CrcValid);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DataReportsCrcValidity(bool validCrc)
    {
        var payload = Enumerable.Repeat((byte)0x5a, MembrainMfmFormat.SectorSize).ToArray();
        var data = Assert.IsType<MembrainMfmData>(MembrainMfmDecoder.TryDecodeData(DataStream(MembrainMfmFormat.DataAddressMark, payload, validCrc), 0));

        Assert.Equal(validCrc, data.CrcValid);
    }

    [Fact]
    public void TruncatedHeaderAndDataAreRejected()
    {
        Assert.Null(MembrainMfmDecoder.TryDecodeHeader(new FluxBitstream(new bool[MembrainMfmFormat.HeaderPatternBitCount], 40), 0));
        Assert.Null(MembrainMfmDecoder.TryDecodeData(new FluxBitstream(new bool[MembrainMfmFormat.DataPatternBitCount], 40), 0));
    }

    [Fact]
    public void MissingDataMarkIsNotFound()
    {
        var stream = new FluxBitstream(new bool[MembrainMfmFormat.DataSearchBitCount], 40);

        Assert.Equal(-1, MembrainMfmDecoder.FindDataMark(stream, 0, stream.Bits.Length));
    }

    [Fact]
    public void UnpairedDataMarkProducesADataStructure()
    {
        var payload = new byte[MembrainMfmFormat.SectorSize];
        var structures = new List<FluxStructure>();

        MembrainMfmDecoder.CollectUnpairedDataMarks(DataStream(MembrainMfmFormat.DataAddressMark, payload, true), new HashSet<int>(), structures);

        Assert.Equal(FluxStructureKind.FormatData, Assert.Single(structures).Kind);
    }

    [Fact]
    public void EncoderRoundTripPreservesPayloadIntegrityStructuresAndConfidence()
    {
        var payload = Enumerable.Range(0, MembrainMfmFormat.SectorSize).Select(index => (byte)(index * 7)).ToArray();
        var encoded = new MembrainMfmTrackEncoder().Encode(new(255, 1, [new(15, payload)]));
        var result = new MembrainMfmDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(result.Sectors);

        Assert.Equal(payload, sector.Data);
        Assert.True(sector.IntegrityValid);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatHeader);
        Assert.Contains(result.Structures, structure => structure.Kind == FluxStructureKind.FormatData);
        Assert.True(result.Confidence > 0);
    }

    private static FluxBitstream HeaderStream(int cylinder, int head, int sector, bool validCrc)
    {
        var address = MembrainMfmAddress.Pack(cylinder, head, sector);
        byte[] header = [MembrainMfmFormat.SyncByte, MembrainMfmFormat.HeaderAddressMark, address.CylinderHigh, address.PackedAddress];
        var crc = Crc16Calculator.Compute(header, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
        if (!validCrc) crc ^= 1;
        var bits = TrackBitEncoding.Bits();
        bits.Raw(MembrainMfmFormat.HeaderPattern.ToArray());
        bits.Mfm([address.CylinderHigh, address.PackedAddress, (byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]);
        return new(bits.ToArray(), 40);
    }

    private static FluxBitstream DataStream(byte mark, IReadOnlyList<byte> payload, bool validCrc)
    {
        var crc = Crc16Calculator.Compute(new[] { MembrainMfmFormat.SyncByte, mark }.Concat(payload), MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
        if (!validCrc) crc ^= 1;
        var bits = TrackBitEncoding.Bits();
        bits.RawHex("4489");
        bits.Mfm(new[] { mark }.Concat(payload).Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        return new(bits.ToArray(), 40);
    }
}
