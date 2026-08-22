using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Geometries.Apple;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie les zones, adresses et ordres sectoriels des géométries Apple.</summary>
public sealed class AppleGeometryTests
{
    [Fact]
    public void LisaZoneBoundariesAndAddressesRemainValid()
    {
        Assert.Equal([22, 22, 21, 21, 20, 20, 19, 19, 18, 18, 17, 17, 16, 16, 15, 15], new[] { 0, 3, 4, 10, 11, 16, 17, 22, 23, 28, 29, 34, 35, 41, 42, 45 }.Select(LisaFileWareGeometry.Sectors));
        Assert.Equal(new(0, 0, 0), LisaFileWareGeometry.Address(0));
        Assert.Equal(0, LisaFileWareGeometry.Address((LisaFileWareGeometry.BlockCount / LisaFileWareGeometry.HeadCount) - 1).Head);
        Assert.Equal(new(0, 1, 0), LisaFileWareGeometry.Address(LisaFileWareGeometry.BlockCount / LisaFileWareGeometry.HeadCount));
        var last = LisaFileWareGeometry.Address(LisaFileWareGeometry.BlockCount - 1);
        Assert.Equal(LisaFileWareGeometry.CylinderCount - 1, last.Cylinder);
        Assert.Equal(LisaFileWareGeometry.HeadCount - 1, last.Head);
        Assert.Throws<InvalidDataException>(() => LisaFileWareGeometry.Address(-1));
        Assert.Throws<InvalidDataException>(() => LisaFileWareGeometry.Address(LisaFileWareGeometry.BlockCount));
        Assert.Throws<ArgumentOutOfRangeException>(() => LisaFileWareGeometry.Sectors(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => LisaFileWareGeometry.Sectors(LisaFileWareGeometry.CylinderCount));
    }

    [Fact]
    public void MacintoshZoneBoundariesHeadsAndAddressesRemainValid()
    {
        Assert.Equal([12, 12, 11, 11, 10, 10, 9, 9, 8, 8], new[] { 0, 15, 16, 31, 32, 47, 48, 63, 64, 79 }.Select(MacintoshGcrGeometry.Sectors));
        Assert.Equal(new(0, 0, 0), MacintoshGcrGeometry.Address(0, MacintoshGcrGeometry.SingleSidedHeadCount));
        Assert.Equal(new(0, 0, 0), MacintoshGcrGeometry.Address(0, MacintoshGcrGeometry.DoubleSidedHeadCount));
        Assert.Equal(new(0, 1, 0), MacintoshGcrGeometry.Address(MacintoshGcrGeometry.MaximumSectorsPerTrack, MacintoshGcrGeometry.DoubleSidedHeadCount));
        var singleLast = MacintoshGcrGeometry.Address(MacintoshGcrGeometry.SingleSidedBlockCount - 1, MacintoshGcrGeometry.SingleSidedHeadCount);
        Assert.Equal(MacintoshGcrGeometry.CylinderCount - 1, singleLast.Cylinder);
        Assert.Equal(0, singleLast.Head);
        var doubleLast = MacintoshGcrGeometry.Address((MacintoshGcrGeometry.SingleSidedBlockCount * MacintoshGcrGeometry.DoubleSidedHeadCount) - 1, MacintoshGcrGeometry.DoubleSidedHeadCount);
        Assert.Equal(MacintoshGcrGeometry.CylinderCount - 1, doubleLast.Cylinder);
        Assert.Equal(MacintoshGcrGeometry.DoubleSidedHeadCount - 1, doubleLast.Head);
        Assert.Throws<ArgumentOutOfRangeException>(() => MacintoshGcrGeometry.Sectors(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => MacintoshGcrGeometry.Sectors(MacintoshGcrGeometry.CylinderCount));
        Assert.Throws<InvalidDataException>(() => MacintoshGcrGeometry.Address(-1, MacintoshGcrGeometry.SingleSidedHeadCount));
        Assert.Throws<InvalidDataException>(() => MacintoshGcrGeometry.Address(MacintoshGcrGeometry.SingleSidedBlockCount, MacintoshGcrGeometry.SingleSidedHeadCount));
        Assert.Throws<ArgumentOutOfRangeException>(() => MacintoshGcrGeometry.Address(0, 3));
    }

    [Fact]
    public void AppleIITablesAreImmutablePermutationsAndConversionPreservesEverySector()
    {
        Assert.Equal(Enumerable.Range(0, AppleIIGeometry.SectorsPerTrack), AppleIIGeometry.ProDosToPhysical.Order());
        Assert.Equal(Enumerable.Range(0, AppleIIGeometry.SectorsPerTrack), AppleIIGeometry.PhysicalToDos.Order());
        Assert.Throws<NotSupportedException>(() => ((IList<int>)AppleIIGeometry.ProDosToPhysical)[0] = 9);
        var source = Enumerable.Range(0, AppleIIGeometry.TrackSize * 2).Select(index => (byte)(index / AppleIIGeometry.SectorSize)).ToArray();
        var converted = AppleIISectorOrderConverter.DosToProDos(source);
        Assert.Equal(source.Order(), converted.Order());
        Assert.Throws<InvalidDataException>(() => AppleIISectorOrderConverter.DosToProDos(new byte[AppleIIGeometry.TrackSize - 1]));
    }
}
