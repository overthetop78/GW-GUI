using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class Fat12DefinitionsTests
{
    [Theory]
    [InlineData(FatDirectoryAttributes.ReadOnly, 0x01)]
    [InlineData(FatDirectoryAttributes.Hidden, 0x02)]
    [InlineData(FatDirectoryAttributes.System, 0x04)]
    [InlineData(FatDirectoryAttributes.VolumeLabel, 0x08)]
    [InlineData(FatDirectoryAttributes.Directory, 0x10)]
    [InlineData(FatDirectoryAttributes.Archive, 0x20)]
    public void DirectoryAttributesKeepTheirOnDiskValues(FatDirectoryAttributes attribute, int value) => Assert.Equal(value, (int)attribute);

    [Fact]
    public void TableDecodesEvenOddAndSpecialEntries()
    {
        var fat = new byte[] { 0xf0, 0xff, 0xff, 0x00, 0x70, 0xff };
        Assert.True(Fat12Table.TryRead(fat, 2, out var even));
        Assert.True(Fat12Table.TryRead(fat, 3, out var odd));
        Assert.True(Fat12Table.TryRead(fat, 1, out var endOfChain));
        Assert.Equal(Fat12Table.FreeCluster, even);
        Assert.Equal(Fat12Table.BadCluster, odd);
        Assert.Equal(Fat12Table.LastEndOfChain, endOfChain);
        Assert.False(Fat12Table.TryRead(fat, 99, out _));
        Assert.InRange(Fat12Table.FirstEndOfChain, Fat12Table.BadCluster + 1, Fat12Table.LastEndOfChain);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.Ibm160, 320, 1, 64, 1)]
    [InlineData(DiskImageFormatIds.Ibm180, 360, 1, 64, 2)]
    [InlineData(DiskImageFormatIds.Ibm320, 640, 2, 112, 1)]
    [InlineData(DiskImageFormatIds.Ibm360, 720, 2, 112, 2)]
    public void LegacyCatalogResolvesEveryLayout(string formatId, int total, int clusters, int roots, int fatSectors)
    {
        Assert.True(IbmLegacyFat12Layout.TryResolve(formatId, out var layout));
        Assert.Equal((total, clusters, roots, fatSectors), (layout.TotalSectors, layout.SectorsPerCluster, layout.RootEntries, layout.SectorsPerFat));
    }

    [Fact]
    public void LayoutValidatesTheFat12ClusterLimit()
    {
        Assert.Equal(708, new Fat12Layout(1, 2, 5, 7, 12, 1, 708).ClusterCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Fat12Layout(1, 2, 5, 7, 12, 1, Fat12Layout.MaximumClusterCount));
    }

    [Fact]
    public void PublicReaderDecodesLabelNameDateAndAttributes()
    {
        var image = CreateImage(Fat12Table.FirstEndOfChain);
        var reader = new Fat12FileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal(string.Empty, volume.Name);
        var entry = Assert.Single(volume.Entries);
        Assert.Equal("FILE.TXT", entry.Name);
        Assert.Equal((uint)(FatDirectoryAttributes.ReadOnly | FatDirectoryAttributes.Hidden | FatDirectoryAttributes.System | FatDirectoryAttributes.Archive), entry.RawAttributes);
        Assert.Equal(new DateTimeOffset(2024, 6, 15, 12, 34, 56, TimeSpan.Zero), entry.Modified);
    }

    [Fact]
    public void PublicReaderReportsCyclicAndMissingChains()
    {
        var cyclic = new Fat12FileSystemReader().Read(CreateImage(Fat12Layout.FirstDataCluster));
        Assert.Contains(cyclic.Warnings, warning => warning.Contains("cyclic FAT chain", StringComparison.Ordinal));
        var outOfRange = new Fat12FileSystemReader().Read(CreateImage(0x700));
        Assert.Contains(outOfRange.Warnings, warning => warning.Contains("Invalid or cyclic FAT chain", StringComparison.Ordinal));
        var complete = CreateImage(Fat12Table.FirstEndOfChain);
        var missing = new SectorImage(complete.FormatId, complete.BlockSize, complete.Cylinders, complete.Heads, complete.SectorsPerTrack, complete.AvailableBlocks.Where(block => block.LogicalBlock != 12));
        var incomplete = new Fat12FileSystemReader().Read(missing);
        Assert.Contains(incomplete.Warnings, warning => warning.Contains("incomplete", StringComparison.Ordinal));
    }

    private static SectorImage CreateImage(int nextCluster)
    {
        const int blockCount = 720;
        var data = Enumerable.Range(0, blockCount).Select(_ => new byte[FatBpbLayout.SectorSize]).ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBpbLayout.BytesPerSectorOffset), FatBpbLayout.SectorSize);
        data[0][FatBpbLayout.SectorsPerClusterOffset] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBpbLayout.ReservedSectorCountOffset), 1);
        data[0][FatBpbLayout.FatCountOffset] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBpbLayout.RootEntryCountOffset), 112);
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBpbLayout.TotalSectors16Offset), blockCount);
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBpbLayout.SectorsPerFatOffset), 2);
        "NO NAME    "u8.CopyTo(data[0].AsSpan(FatBpbLayout.VolumeLabelOffset));
        data[1][0] = 0xf9;
        data[1][1] = 0xff;
        data[1][2] = 0xff;
        data[1][3] = (byte)(nextCluster & 0xff);
        data[1][4] = (byte)(nextCluster >> 8 & 0x0f);
        "FILE    TXT"u8.CopyTo(data[5]);
        data[5][FatDirectoryLayout.AttributesOffset] = (byte)(FatDirectoryAttributes.ReadOnly | FatDirectoryAttributes.Hidden | FatDirectoryAttributes.System | FatDirectoryAttributes.Archive);
        BinaryPrimitives.WriteUInt16LittleEndian(data[5].AsSpan(FatDirectoryLayout.FirstClusterOffset), Fat12Layout.FirstDataCluster);
        BinaryPrimitives.WriteUInt32LittleEndian(data[5].AsSpan(FatDirectoryLayout.FileSizeOffset), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(data[5].AsSpan(FatDirectoryLayout.ModifiedDateOffset), (ushort)((2024 - 1980) << 9 | 6 << 5 | 15));
        BinaryPrimitives.WriteUInt16LittleEndian(data[5].AsSpan(FatDirectoryLayout.ModifiedTimeOffset), (ushort)(12 << 11 | 34 << 5 | 28));
        data[12][0] = 0x42;
        var blocks = data.Select((bytes, index) => new SectorBlock(index, new SectorAddress(0, 0, index), bytes));
        return new SectorImage(DiskImageFormatIds.Ibm360, FatBpbLayout.SectorSize, 40, 2, 9, blocks);
    }
}
