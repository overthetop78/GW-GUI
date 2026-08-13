using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Commodore;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Valide les pistes SCP Commodore zonées produites depuis le modèle sectoriel.</summary>
public sealed class CommodoreSectorImageScpConversionTests
{
    public static TheoryData<string, int> CommodoreDosFormats => new()
    {
        { DiskImageFormatIds.Commodore1541, 1 },
        { DiskImageFormatIds.Commodore1571, 2 }
    };

    [Theory]
    [MemberData(nameof(CommodoreDosFormats))]
    public void CommodoreDosZonesRoundTripEverySector(string formatId, int heads)
    {
        int[] cylinders = [0, 17, 24, 30, 39];
        var source = CreateCommodoreDosImage(formatId, heads, cylinders);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);

        Assert.Equal((byte)ScpDiskType.Commodore64, scp.Header.DiskType);
        Assert.Equal(cylinders.Length * heads, scp.Tracks.Count);
        foreach (var track in scp.Tracks)
        {
            var revolution = Assert.Single(track.Revolutions);
            Assert.Equal(TrackEncodingTimings.Rpm300IndexTimeTicks, revolution.IndexTimeTicks);
            Assert.True(revolution.FluxIntervals.Sum(interval => (long)interval) <= revolution.IndexTimeTicks);
            AssertCommodoreDosTrack(source, track, revolution);
        }
    }

    [Fact]
    public void Commodore900ZonesRoundTripEverySector()
    {
        int[] cylinders = [0, Commodore900Geometry.Zone2StartCylinder, Commodore900Geometry.Zone3StartCylinder, Commodore900Geometry.Zone4StartCylinder];
        var source = CreateCommodore900Image(cylinders);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);

        Assert.Equal((byte)ScpDiskType.Other1200, scp.Header.DiskType);
        Assert.Equal(cylinders.Length * Commodore900Geometry.HeadCount, scp.Tracks.Count);
        foreach (var track in scp.Tracks)
        {
            var revolution = Assert.Single(track.Revolutions);
            Assert.Equal(TrackEncodingTimings.Rpm300IndexTimeTicks, revolution.IndexTimeTicks);
            Assert.True(revolution.FluxIntervals.Sum(interval => (long)interval) <= revolution.IndexTimeTicks);
            AssertCommodore900Track(source, track, revolution);
        }
    }

    [Fact]
    public void CommodorePoliciesUseThePhysicalCadenceOfEveryZone()
    {
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore1541Zone1BitCellTicks, CommodoreTrackEncodingTimings.Commodore1541BitCellTicks(0));
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore1541Zone2BitCellTicks, CommodoreTrackEncodingTimings.Commodore1541BitCellTicks(17));
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore1541Zone3BitCellTicks, CommodoreTrackEncodingTimings.Commodore1541BitCellTicks(24));
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore1541Zone4BitCellTicks, CommodoreTrackEncodingTimings.Commodore1541BitCellTicks(30));
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore900Zone1BitCellTicks, CommodoreTrackEncodingTimings.Commodore900BitCellTicks(0));
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore900Zone2BitCellTicks, CommodoreTrackEncodingTimings.Commodore900BitCellTicks(Commodore900Geometry.Zone2StartCylinder));
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore900Zone3BitCellTicks, CommodoreTrackEncodingTimings.Commodore900BitCellTicks(Commodore900Geometry.Zone3StartCylinder));
        Assert.Equal(CommodoreTrackEncodingTimings.Commodore900Zone4BitCellTicks, CommodoreTrackEncodingTimings.Commodore900BitCellTicks(Commodore900Geometry.Zone4StartCylinder));
    }

    private static SectorImage CreateCommodoreDosImage(string formatId, int heads, IReadOnlyList<int> cylinders)
    {
        var blocks = new List<SectorBlock>();
        foreach (var cylinder in cylinders)
            for (var head = 0; head < heads; head++)
                for (var sector = 0; sector < Commodore1541Geometry.SectorsPerTrack(cylinder + Commodore1541Geometry.FirstTrack); sector++) blocks.Add(Block(blocks.Count, cylinder, head, sector, Commodore1541Geometry.SectorSize));
        return new(formatId, Commodore1541Geometry.SectorSize, Commodore1541Geometry.ExtendedTrackCount, heads, Commodore1541Geometry.MaximumSectorsPerTrack, blocks);
    }

    private static SectorImage CreateCommodore900Image(IReadOnlyList<int> cylinders)
    {
        var blocks = new List<SectorBlock>();
        foreach (var cylinder in cylinders)
            for (var head = 0; head < Commodore900Geometry.HeadCount; head++)
                for (var sector = 0; sector < Commodore900Geometry.SectorsPerTrack(cylinder); sector++) blocks.Add(Block(blocks.Count, cylinder, head, sector, Commodore900Geometry.SectorSize));
        return new(DiskImageFormatIds.Commodore900Coherent, Commodore900Geometry.SectorSize, Commodore900Geometry.CylinderCount, Commodore900Geometry.HeadCount, Commodore900Geometry.MaximumSectorsPerTrack, blocks, capacity: Commodore900Geometry.Capacity);
    }

    private static SectorBlock Block(int logical, int cylinder, int head, int sector, int size)
    {
        var seed = cylinder * 7 + head * 53 + sector + 1;
        var data = Enumerable.Range(0, size).Select(index => unchecked((byte)(seed + index * 3))).ToArray();
        return new(logical, new(cylinder, head, sector), data, true);
    }

    private static void AssertCommodoreDosTrack(SectorImage source, ScpTrack track, ScpRevolution revolution)
    {
        var diskTrack = track.Cylinder + Commodore1541Geometry.FirstTrack + track.Head * source.Cylinders;
        var decoded = new FluxDecoderRegistry().Decode(FluxCodecIds.CommodoreGcr, revolution.Flux).Sectors.Where(sector => sector.Cylinder == diskTrack && sector.IntegrityValid == true && sector.Data is not null).ToDictionary(sector => sector.Number);
        AssertTrackData(source, track, decoded);
    }

    private static void AssertCommodore900Track(SectorImage source, ScpTrack track, ScpRevolution revolution)
    {
        var decoded = new FluxDecoderRegistry().Decode(FluxCodecIds.Commodore900Gcr, revolution.Flux).Sectors.Where(sector => sector.Cylinder == track.Cylinder && sector.IntegrityValid == true && sector.Data is not null).ToDictionary(sector => sector.Number);
        AssertTrackData(source, track, decoded);
    }

    private static void AssertTrackData(SectorImage source, ScpTrack track, IReadOnlyDictionary<int, DecodedSector> decoded)
    {
        foreach (var block in source.AvailableBlocks.Where(block => block.Address.Cylinder == track.Cylinder && block.Address.Head == track.Head))
        {
            Assert.True(decoded.TryGetValue(block.Address.Number, out var sector), $"Secteur Commodore absent : {block.Address}.");
            Assert.Equal(block.Data, sector.Data);
        }
    }
}
