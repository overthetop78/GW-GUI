using GWGUI.Scp.Decoding;
using GWGUI.Scp.Encoding;

namespace GWGUI.Tests;

public sealed class TrackEncoderTests
{
    public static TheoryData<string, int> RoundTrips => new()
    {
        { "iso.mfm", 512 }, { "iso.fm", 256 }, { "amiga.mfm", 512 },
        { "apple2.gcr", 256 }, { "applemac.gcr", 512 }, { "commodore.gcr", 256 },
        { "hp.mmfm", 256 }, { "datageneral.fm", 512 }, { "micropolis.mfm", 256 },
        { "membrain.mfm", 512 }, { "aed6200p.mfm", 512 }, { "qdmo5.mfm", 128 },
        { "centurion.mfm", 256 }, { "northstar.mfm", 512 }, { "heathkit.fm", 256 },
        { "micraln.fm", 128 }, { "emu.fm", 0xe00 }, { "tycom.fm", 128 },
        { "dec.rx02", 128 }, { "victor9k.gcr", 512 }
    };

    [Fact]
    public void RegistryContainsEncoderForEverySemanticDecoder()
    {
        var decoderIds = new FluxDecoderRegistry().Decoders.Where(item => item.Id != "raw").Select(item => item.Id).Order().ToArray();
        var encoderIds = new FluxEncoderRegistry().Encoders.Select(item => item.Id).Order().ToArray();
        Assert.Equal(decoderIds, encoderIds);
    }

    [Theory]
    [MemberData(nameof(RoundTrips))]
    public void EncodedTrackIsRecognizedByMatchingDecoder(string id, int size)
    {
        var data = Enumerable.Range(0, size).Select(index => (byte)(index * 37 + 11)).ToArray();
        var sectorNumber = id == "qdmo5.mfm" ? 0x123 : 3;
        var request = new TrackEncodeRequest(2, 0, [new TrackSector(sectorNumber, data)]);
        var encoded = new FluxEncoderRegistry().Encode(id, request);

        var decoded = new FluxDecoderRegistry().Decode(id, encoded.Revolution);

        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid, string.Join(" | ", decoded.Structures.Select(item => item.Description)));
        Assert.Equal(id == "emu.fm" ? 1 : sectorNumber, sector.Number);
        var payload = id == "commodore.gcr" ? decoded.DecodedBytes.Skip(7).Take(size) : decoded.DecodedBytes.TakeLast(size);
        Assert.Equal(data, payload);
    }

    [Fact]
    public void ArburgDataTrackRoundTrips()
    {
        var data = Enumerable.Range(0, 0x9fe).Select(index => (byte)(index * 7)).ToArray();
        var encoded = new FluxEncoderRegistry().Encode("arburg", new TrackEncodeRequest(0, 0, [new TrackSector(1, data)]));
        var decoded = new FluxDecoderRegistry().Decode("arburg", encoded.Revolution);
        Assert.True(Assert.Single(decoded.Sectors!).IntegrityValid);
        Assert.Equal(data, decoded.DecodedBytes.TakeLast(data.Length));
    }

    [Fact]
    public void ArburgSystemTrackRoundTrips()
    {
        var data = Enumerable.Range(0, 0xefe).Select(index => (byte)(index * 5)).ToArray();
        var attributes = new Dictionary<string, int> { ["system"] = 1 };
        var encoded = new FluxEncoderRegistry().Encode("arburg", new TrackEncodeRequest(0, 0, [new TrackSector(1, data, Attributes: attributes)]));
        var decoded = new FluxDecoderRegistry().Decode("arburg", encoded.Revolution);
        Assert.True(Assert.Single(decoded.Sectors!).IntegrityValid);
        Assert.Equal(data, decoded.DecodedBytes.TakeLast(data.Length));
    }

    [Fact]
    public void DecRx02M2FmTrackRoundTrips()
    {
        var data = Enumerable.Range(0, 256).Select(index => (byte)(index * 13)).ToArray();
        var encoded = new FluxEncoderRegistry().Encode("dec.rx02", new TrackEncodeRequest(4, 1, [new TrackSector(6, data)]));
        var decoded = new FluxDecoderRegistry().Decode("dec.rx02", encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid, string.Join(" | ", decoded.Structures.Select(item => item.Description)));
        Assert.Equal(data, decoded.DecodedBytes.TakeLast(data.Length));
    }

    [Theory]
    [InlineData("iso.mfm")]
    [InlineData("iso.fm")]
    public void IsoDeletedSectorRoundTrips(string id)
    {
        var data = Enumerable.Range(0, 128).Select(index => (byte)(index + 1)).ToArray();
        var encoded = new FluxEncoderRegistry().Encode(id, new TrackEncodeRequest(1, 0, [new TrackSector(2, data, Deleted: true)]));
        var decoded = new FluxDecoderRegistry().Decode(id, encoded.Revolution);
        Assert.True(Assert.Single(decoded.Sectors!).IntegrityValid);
        Assert.Contains(decoded.Structures, item => item.Kind == FluxStructureKind.DeletedDataAddressMark);
    }
}
