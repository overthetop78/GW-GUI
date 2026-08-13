using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Dec;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Valide le raccordement des formats sectoriels rares au service de reconstruction SCP.</summary>
public sealed class RareSectorImageScpConversionTests
{
    public static TheoryData<string, string, int> RareFormats => new()
    {
        { DiskImageFormatIds.HpMmfm, FluxCodecIds.HpMmfm, HpMmfmFormat.SectorSize },
        { DiskImageFormatIds.DataGeneralFm, FluxCodecIds.DataGeneralFm, DataGeneralFmFormat.SectorSize },
        { DiskImageFormatIds.MicropolisMfm, FluxCodecIds.MicropolisMfm, MicropolisMfmFormat.SectorSize },
        { DiskImageFormatIds.MembrainMfm, FluxCodecIds.MembrainMfm, MembrainMfmFormat.SectorSize },
        { DiskImageFormatIds.Aed6200pMfm, FluxCodecIds.Aed6200pMfm, 256 },
        { DiskImageFormatIds.QdMo5Mfm, FluxCodecIds.QdMo5Mfm, QdMo5MfmFormat.SectorSize },
        { DiskImageFormatIds.CenturionMfm, FluxCodecIds.CenturionMfm, 256 },
        { DiskImageFormatIds.NorthstarMfm, FluxCodecIds.NorthstarMfm, NorthstarMfmFormat.SectorSize },
        { DiskImageFormatIds.HeathkitFm, FluxCodecIds.HeathkitFm, HeathkitFmFormat.SectorSize },
        { DiskImageFormatIds.MicralNFm, FluxCodecIds.MicralNFm, MicralNFmFormat.SectorSize },
        { DiskImageFormatIds.EmuFm, FluxCodecIds.EmuFm, EmuFmFormat.SectorSize },
        { DiskImageFormatIds.TycomFm, FluxCodecIds.TycomFm, TycomFmFormat.SectorSize },
        { DiskImageFormatIds.Arburg, FluxCodecIds.Arburg, ArburgFormat.DataUsefulSize },
        { DiskImageFormatIds.Victor9kGcr, FluxCodecIds.Victor9kGcr, Victor9kGcrFormat.SectorByteCount }
    };

    [Theory]
    [MemberData(nameof(RareFormats))]
    public void RareFormatRoundTripsThroughTheCommonScpService(string formatId, string codecId, int sectorSize)
    {
        var payload = Payload(sectorSize);
        var image = Image(formatId, sectorSize, payload);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(image);

        var track = Assert.Single(scp.Tracks);
        var revolution = Assert.Single(track.Revolutions);
        var decoded = new FluxDecoderRegistry().Decode(codecId, revolution.Flux);
        var sector = Assert.Single(decoded.Sectors, item => item.IntegrityValid == true && item.Data is not null);
        Assert.Equal(payload, sector.Data);
    }

    [Fact]
    public void DecRx02LogicalBlockRoundTripsAsTwoPhysicalSectors()
    {
        var payload = Payload(DecRx02Geometry.LogicalBlockSize);
        var image = Image(DiskImageFormatIds.DecRx02, DecRx02Geometry.LogicalBlockSize, payload);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(image);

        var revolution = Assert.Single(Assert.Single(scp.Tracks).Revolutions);
        var decoded = new FluxDecoderRegistry().Decode(FluxCodecIds.DecRx02, revolution.Flux);
        var sectors = decoded.Sectors.Where(item => item.IntegrityValid == true && item.Data is not null).OrderBy(item => item.Number).ToArray();
        Assert.Equal(DecRx02Geometry.PhysicalSectorsPerLogicalBlock, sectors.Length);
        Assert.Equal(payload, sectors.SelectMany(item => item.Data!).ToArray());
    }

    private static SectorImage Image(string formatId, int sectorSize, byte[] payload) =>
        new(formatId, sectorSize, 1, 1, 1, [new SectorBlock(0, new(0, 0, 1), payload, true)]);

    private static byte[] Payload(int size) =>
        Enumerable.Range(0, size).Select(index => unchecked((byte)(index * 17 + 3))).ToArray();
}
