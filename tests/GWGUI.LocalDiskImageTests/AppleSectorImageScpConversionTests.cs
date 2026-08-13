using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Apple;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Valide les pistes SCP Apple II, Macintosh et Lisa produites depuis le modèle sectoriel.</summary>
public sealed class AppleSectorImageScpConversionTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIDos32, 13, ScpDiskType.AppleII)]
    [InlineData(DiskImageFormatIds.AppleIIDos33, 16, ScpDiskType.AppleII)]
    [InlineData(DiskImageFormatIds.AppleIIProDos140, 16, ScpDiskType.AppleIIProDos)]
    public void AppleIIGcrTracksRoundTripEverySector(string formatId, int sectorCount, ScpDiskType diskType)
    {
        var source = CreateTrackImage(formatId, 35, 1, sectorCount, 256, 0);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);
        Assert.Equal((byte)diskType, scp.Header.DiskType);
        var revolution = Assert.Single(Assert.Single(scp.Tracks).Revolutions);
        Assert.Equal(TrackEncodingTimings.Rpm300IndexTimeTicks, revolution.IndexTimeTicks);
        AssertTrack(source, revolution, FluxCodecIds.AppleIIGcr, compareTags: false);
    }

    [Fact]
    public void Rwts18UsesItsPhysicalOrderAndRoundTripsEverySector()
    {
        var source = CreateTrackImage(DiskImageFormatIds.AppleIIRwts18, 35, 1, AppleRwts18Format.SectorCount, AppleRwts18Format.SectorByteCount, 0);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);
        var revolution = Assert.Single(Assert.Single(scp.Tracks).Revolutions);
        AssertTrack(source, revolution, FluxCodecIds.AppleRwts18, compareTags: false);
    }

    [Fact]
    public void AppleIIISosBlocksRoundTripThroughSixteenPhysicalSectors()
    {
        var source = CreateTrackImage(DiskImageFormatIds.AppleIIISos, 35, 1, AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.ProDosBlockSize, 0);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);
        var revolution = Assert.Single(Assert.Single(scp.Tracks).Revolutions);
        var decoded = new FluxDecoderRegistry().Decode(FluxCodecIds.AppleIIGcr, revolution.Flux).Sectors.Where(sector => sector.IntegrityValid == true && sector.Data is not null).ToDictionary(sector => sector.Number);
        foreach (var block in source.AvailableBlocks)
        {
            Assert.Equal(block.Data.Take(AppleIIGeometry.SectorSize), decoded[block.Address.Number * AppleIIGeometry.SectorsPerProDosBlock].Data);
            Assert.Equal(block.Data.Skip(AppleIIGeometry.SectorSize), decoded[block.Address.Number * AppleIIGeometry.SectorsPerProDosBlock + 1].Data);
        }
    }

    [Fact]
    public void MacintoshZonesKeepTheirSectorCountsSpeedsAndTags()
    {
        int[] cylinders = [0, 16, 32, 48, 64];
        uint[] indexTimes = [AppleTrackEncodingTimings.MacintoshZone1IndexTimeTicks, AppleTrackEncodingTimings.MacintoshZone2IndexTimeTicks, AppleTrackEncodingTimings.MacintoshZone3IndexTimeTicks, AppleTrackEncodingTimings.MacintoshZone4IndexTimeTicks, AppleTrackEncodingTimings.MacintoshZone5IndexTimeTicks];
        var source = CreateZonedImage(DiskImageFormatIds.Mac400, MacintoshGcrGeometry.CylinderCount, MacintoshGcrGeometry.SingleSidedHeadCount, cylinders, MacintoshGcrGeometry.Sectors);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);
        Assert.Equal((byte)ScpDiskType.AppleMacintosh400, scp.Header.DiskType);
        Assert.Equal(cylinders.Length, scp.Tracks.Count);
        for (var index = 0; index < cylinders.Length; index++)
        {
            var track = Assert.Single(scp.Tracks, track => track.Cylinder == cylinders[index]);
            var revolution = Assert.Single(track.Revolutions);
            Assert.Equal(indexTimes[index], revolution.IndexTimeTicks);
            AssertTrack(source, revolution, FluxCodecIds.AppleMacGcr, compareTags: true, cylinder: cylinders[index]);
        }
    }

    [Fact]
    public void LisaFileWareKeepsTagsAndUsesItsZonedIndexDuration()
    {
        var source = CreateZonedImage(DiskImageFormatIds.AppleLisaOffice, LisaFileWareGeometry.CylinderCount, LisaFileWareGeometry.HeadCount, [0, LisaFileWareGeometry.Zone7End], LisaFileWareGeometry.Sectors);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);
        foreach (var track in scp.Tracks)
        {
            var revolution = Assert.Single(track.Revolutions);
            Assert.Equal(AppleTrackEncodingTimings.LisaIndexTimeTicks(track.Cylinder), revolution.IndexTimeTicks);
            AssertTrack(source, revolution, FluxCodecIds.AppleLisaFileWareGcr, compareTags: true, cylinder: track.Cylinder);
        }
    }

    [Fact]
    public void AppleIIProDos800UsesTheMacintoshGcrProfile()
    {
        var source = CreateTrackImage(DiskImageFormatIds.AppleIIProDos800, MacintoshGcrGeometry.CylinderCount, MacintoshGcrGeometry.DoubleSidedHeadCount, MacintoshGcrGeometry.MaximumSectorsPerTrack, MacintoshGcrGeometry.BlockSize, 0, withTags: true);
        var scp = MediaEngineFactory.CreateSectorImageScpConversionService().Create(source);
        Assert.Equal((byte)ScpDiskType.AppleMacintosh800, scp.Header.DiskType);
        AssertTrack(source, Assert.Single(Assert.Single(scp.Tracks).Revolutions), FluxCodecIds.AppleMacGcr, compareTags: true);
    }

    private static SectorImage CreateTrackImage(string formatId, int cylinders, int heads, int sectorsPerTrack, int sectorSize, int firstSector, bool withTags = false)
    {
        var blocks = Enumerable.Range(0, sectorsPerTrack).Select(index => Block(index, 0, firstSector + index, sectorSize, withTags)).ToArray();
        return new(formatId, sectorSize, cylinders, heads, sectorsPerTrack, blocks);
    }

    private static SectorImage CreateZonedImage(string formatId, int cylinders, int heads, IReadOnlyList<int> selectedCylinders, Func<int, int> sectorCount)
    {
        var blocks = new List<SectorBlock>();
        foreach (var cylinder in selectedCylinders)
            for (var sector = 0; sector < sectorCount(cylinder); sector++) blocks.Add(Block(blocks.Count, cylinder, sector, MacintoshGcrGeometry.BlockSize, withTags: true));
        return new(formatId, MacintoshGcrGeometry.BlockSize, cylinders, heads, selectedCylinders.Max(sectorCount), blocks);
    }

    private static SectorBlock Block(int logical, int cylinder, int sector, int size, bool withTags)
    {
        var data = Enumerable.Repeat(checked((byte)(logical + 1)), size).ToArray();
        var tag = withTags ? Enumerable.Range(0, AppleIwmGcrFormat.TagByteCount).Select(value => checked((byte)(value + sector))).ToArray() : null;
        return new(logical, new(cylinder, 0, sector), data, Tag: tag);
    }

    private static void AssertTrack(SectorImage source, ScpRevolution revolution, string codecId, bool compareTags, int cylinder = 0)
    {
        Assert.True(revolution.FluxIntervals.Sum(interval => (long)interval) <= revolution.IndexTimeTicks, $"La piste Apple {cylinder} dépasse sa durée d'index.");
        var decoded = new FluxDecoderRegistry().Decode(codecId, revolution.Flux).Sectors.Where(sector => sector.Cylinder == cylinder && sector.IntegrityValid == true && sector.Data is not null).ToDictionary(sector => sector.Number);
        foreach (var block in source.AvailableBlocks.Where(block => block.Address.Cylinder == cylinder))
        {
            Assert.True(decoded.TryGetValue(block.Address.Number, out var sector), $"Secteur Apple absent : {block.Address}.");
            Assert.Equal(block.Data, sector.Data);
            if (compareTags) Assert.Equal(block.Tag, sector.Tag);
        }
    }
}
