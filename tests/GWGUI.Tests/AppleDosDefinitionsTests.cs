using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Dos;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions et erreurs Apple DOS par le lecteur public.</summary>
public sealed class AppleDosDefinitionsTests
{
    /// <summary>Vérifie les géométries Apple DOS 3.2 et 3.3.</summary>
    [Theory]
    [InlineData(AppleDosFileSystemLayout.Dos32SectorsPerTrack, "Apple DOS 3.2")]
    [InlineData(AppleDosFileSystemLayout.Dos33SectorsPerTrack, "Apple DOS 3.3")]
    public void ReadsBothAppleDosGeometries(int sectorsPerTrack, string expected) => Assert.Equal(expected, new AppleDosFileSystemReader().Read(BuildImage(sectorsPerTrack, AppleDosFileType.Text)).FileSystem);

    /// <summary>Vérifie chaque type de fichier et le décodage du nom à bit fort.</summary>
    [Theory]
    [InlineData(0, "Text")]
    [InlineData(1, "Integer BASIC")]
    [InlineData(2, "Applesoft BASIC")]
    [InlineData(4, "Binary")]
    [InlineData(8, "S")]
    [InlineData(16, "Relocatable")]
    [InlineData(32, "A")]
    [InlineData(64, "B")]
    public void ReadsEveryFileTypeAndHighBitName(int rawType, string expected)
    {
        var type = (AppleDosFileType)rawType;
        var entry = Assert.Single(new AppleDosFileSystemReader().Read(BuildImage(AppleDosFileSystemLayout.Dos33SectorsPerTrack, type)).Entries);
        Assert.Equal("HELLO", entry.Name);
        Assert.Equal(expected, entry.Comment);
    }

    /// <summary>Vérifie une chaîne de listes T/S, un cycle et un secteur de données absent.</summary>
    [Fact]
    public void ReportsTrackSectorChainFailures()
    {
        var chained = BuildImage(AppleDosFileSystemLayout.Dos33SectorsPerTrack, AppleDosFileType.Binary, secondList: true);
        Assert.Equal(2 * AppleDosFileSystemLayout.SectorSize, Assert.Single(new AppleDosFileSystemReader().Read(chained).Entries).Content!.Count);

        var cyclic = BuildImage(AppleDosFileSystemLayout.Dos33SectorsPerTrack, AppleDosFileType.Binary, cycle: true);
        Assert.Contains(new AppleDosFileSystemReader().Read(cyclic).Warnings, warning => warning.Contains("cyclic", StringComparison.Ordinal));

        var missingLogical = 2 * AppleDosFileSystemLayout.Dos33SectorsPerTrack;
        var missingSource = BuildImage(AppleDosFileSystemLayout.Dos33SectorsPerTrack, AppleDosFileType.Binary);
        var missing = new SectorImage(missingSource.FormatId, missingSource.BlockSize, missingSource.Cylinders, missingSource.Heads, missingSource.SectorsPerTrack, missingSource.AvailableBlocks.Where(block => block.LogicalBlock != missingLogical));
        Assert.Contains(new AppleDosFileSystemReader().Read(missing).Warnings, warning => warning.Contains("data sector", StringComparison.Ordinal));
    }

    private static SectorImage BuildImage(int sectorsPerTrack, AppleDosFileType type, bool secondList = false, bool cycle = false)
    {
        var data = new byte[AppleDosFileSystemLayout.TrackCount * sectorsPerTrack * AppleDosFileSystemLayout.SectorSize];
        var vtoc = AppleDosFileSystemLayout.VtocTrack * sectorsPerTrack * AppleDosFileSystemLayout.SectorSize;
        var catalogSector = sectorsPerTrack - 1;
        data[vtoc + AppleDosFileSystemLayout.VtocCatalogTrackOffset] = AppleDosFileSystemLayout.VtocTrack;
        data[vtoc + AppleDosFileSystemLayout.VtocCatalogSectorOffset] = (byte)catalogSector;
        data[vtoc + AppleDosFileSystemLayout.VtocTrackCountOffset] = AppleDosFileSystemLayout.TrackCount;
        data[vtoc + AppleDosFileSystemLayout.VtocSectorsPerTrackOffset] = (byte)sectorsPerTrack;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(vtoc + AppleDosFileSystemLayout.VtocSectorSizeOffset), AppleDosFileSystemLayout.SectorSize);
        var catalog = (AppleDosFileSystemLayout.VtocTrack * sectorsPerTrack + catalogSector) * AppleDosFileSystemLayout.SectorSize;
        var entry = catalog + AppleDosFileSystemLayout.CatalogFirstEntryOffset;
        data[entry + AppleDosFileSystemLayout.EntryTrackOffset] = 1;
        data[entry + AppleDosFileSystemLayout.EntrySectorOffset] = 0;
        data[entry + AppleDosFileSystemLayout.EntryTypeOffset] = (byte)type;
        var name = "HELLO".PadRight(AppleDosFileSystemLayout.EntryNameLength).Select(character => (byte)(character | 0x80)).ToArray();
        name.CopyTo(data, entry + AppleDosFileSystemLayout.EntryNameOffset);
        var list = (1 * sectorsPerTrack) * AppleDosFileSystemLayout.SectorSize;
        if (cycle) { data[list + 1] = 1; data[list + 2] = 0; }
        else if (secondList) { data[list + 1] = 1; data[list + 2] = 1; }
        data[list + AppleDosFileSystemLayout.TrackSectorPairsOffset] = 2;
        data[list + AppleDosFileSystemLayout.TrackSectorPairsOffset + 1] = 0;
        data.AsSpan(2 * sectorsPerTrack * AppleDosFileSystemLayout.SectorSize, AppleDosFileSystemLayout.SectorSize).Fill(0x41);
        if (secondList)
        {
            var next = (1 * sectorsPerTrack + 1) * AppleDosFileSystemLayout.SectorSize;
            data[next + AppleDosFileSystemLayout.TrackSectorPairsOffset] = 2;
            data[next + AppleDosFileSystemLayout.TrackSectorPairsOffset + 1] = 1;
            data.AsSpan((2 * sectorsPerTrack + 1) * AppleDosFileSystemLayout.SectorSize, AppleDosFileSystemLayout.SectorSize).Fill(0x42);
        }
        var blocks = Enumerable.Range(0, AppleDosFileSystemLayout.TrackCount * sectorsPerTrack).Select(logical => new SectorBlock(logical, new(logical / sectorsPerTrack, 0, logical % sectorsPerTrack), data.AsSpan(logical * AppleDosFileSystemLayout.SectorSize, AppleDosFileSystemLayout.SectorSize).ToArray()));
        return new(sectorsPerTrack == AppleDosFileSystemLayout.Dos32SectorsPerTrack ? DiskImageFormatIds.AppleIIDos32 : DiskImageFormatIds.AppleIIDos33, AppleDosFileSystemLayout.SectorSize, AppleDosFileSystemLayout.TrackCount, 1, sectorsPerTrack, blocks);
    }
}
