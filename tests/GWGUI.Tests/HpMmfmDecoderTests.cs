using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions et les transformations HP MMFM.</summary>
public sealed class HpMmfmDecoderTests
{
    /// <summary>Vérifie les deux synchronisations techniques.</summary>
    [Fact]
    public void SynchronizationsAreDefined()
    {
        Assert.Equal([0x55, 0x55, 0x2a, 0x54], HpMmfmFormat.SectorSync);
        Assert.Equal([0x55, 0x55, 0x2a, 0x44], HpMmfmFormat.DataSync);
    }

    /// <summary>Vérifie l'inversion et l'échange des octets par paires.</summary>
    [Fact]
    public void PayloadTransformationRoundTrips()
    {
        var payload = Enumerable.Range(0, HpMmfmFormat.SectorSize).Select(index => (byte)(index * 17)).ToArray();
        var encoded = HpMmfmCodec.EncodePayload(payload);

        Assert.Equal(BitPrimitives.ReverseBits(payload[1]), encoded[0]);
        Assert.Equal(payload, HpMmfmCodec.DecodePayload(encoded));
    }

    /// <summary>Vérifie un en-tête valide puis invalide et son identité.</summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HeaderReportsCrcAndIdentity(bool valid)
    {
        byte cylinder = 12, head = 1, sector = 5;
        byte[] identity = [BitPrimitives.ReverseBits(cylinder), BitPrimitives.ReverseBits((byte)(sector | head << HpMmfmFormat.HeadShift))];
        var header = Crc16Calculator.Append(identity);
        if (!valid) header[^1] ^= byte.MaxValue;
        var bits = TrackBitEncoding.Bits();
        bits.Raw(HpMmfmFormat.SectorSync.ToArray());
        bits.Mfm(header);

        var decoded = Assert.IsType<HpMmfmHeader>(HpMmfmDecoder.TryDecodeHeader(new FluxBitstream(bits.ToArray(), 40), 0));
        Assert.Equal(cylinder, decoded.Cylinder);
        Assert.Equal(head, decoded.Head);
        Assert.Equal(sector, decoded.Sector);
        Assert.Equal(valid, decoded.CrcValid);
    }

    /// <summary>Vérifie les bornes minimale et maximale de recherche des données.</summary>
    [Theory]
    [InlineData(160)]
    [InlineData(928)]
    public void DataSyncIsFoundAtSearchBounds(int targetOffset)
    {
        var bits = Enumerable.Repeat(false, targetOffset + HpMmfmFormat.SyncBitCount).ToList();
        var sync = HpMmfmFormat.DataSync.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & 1 << (7 - bit)) != 0)).ToArray();
        for (var index = 0; index < sync.Length; index++) bits[targetOffset + index] = sync[index];

        Assert.Equal(targetOffset, HpMmfmDecoder.FindDataSync(new FluxBitstream(bits.ToArray(), 40), 0));
    }

    /// <summary>Vérifie qu'un bloc tronqué ne peut pas être décodé.</summary>
    [Fact]
    public void TruncatedDataIsRejected()
    {
        var stream = new FluxBitstream(new bool[HpMmfmFormat.SyncBitCount + HpMmfmFormat.EncodedByteBitCount], 40);

        Assert.Null(HpMmfmDecoder.TryDecodeData(stream, 0));
    }

    /// <summary>Vérifie l'absence de synchronisation et l'utilisation unique d'un bloc de données partagé.</summary>
    [Fact]
    public void MissingAndAlreadyUsedDataSynchronizationsAreHandled()
    {
        Assert.Equal(-1, HpMmfmDecoder.FindDataSync(new FluxBitstream(new bool[HpMmfmFormat.MaximumDataSearchOffsetBits + HpMmfmFormat.SyncBitCount], 40), 0));
        byte[] Header(byte sector) => Crc16Calculator.Append([BitPrimitives.ReverseBits((byte)1), BitPrimitives.ReverseBits(sector)]);
        var payload = HpMmfmCodec.EncodePayload(new byte[HpMmfmFormat.SectorSize]);
        var bits = TrackBitEncoding.Bits();
        bits.Raw(HpMmfmFormat.SectorSync.ToArray());
        bits.Mfm(Header(1));
        bits.Gap(64);
        bits.Raw(HpMmfmFormat.SectorSync.ToArray());
        bits.Mfm(Header(2));
        bits.Gap(64);
        bits.Raw(HpMmfmFormat.DataSync.ToArray());
        bits.Mfm(Crc16Calculator.Append(payload));
        bits.Gap(1, true);

        var result = new HpMmfmDecoder().Decode(GWGUI.MediaEngine.Flux.FluxRevolutionFactory.Create(bits, 40, 8_000_000));
        Assert.Equal(2, result.Sectors.Count);
        Assert.Single(result.Sectors, sector => sector.Data is not null);
    }
}
