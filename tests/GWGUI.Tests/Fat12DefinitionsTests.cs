using GWGUI.MediaEngine.Exploration;
using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Images;

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
        Assert.True(Fat12LegacyLayoutCatalog.TryResolve(formatId, out var layout));
        Assert.Equal((total, clusters, roots, fatSectors), (layout.TotalSectors, layout.SectorsPerCluster, layout.RootEntries, layout.SectorsPerFat));
    }

    [Fact]
    public void LayoutValidatesTheFat12ClusterLimit()
    {
        Assert.Equal(708, new Fat12Layout(1, 2, 5, 7, 12, 1, 708).ClusterCount);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Fat12Layout(1, 2, 5, 7, 12, 1, Fat12Layout.MaximumClusterCount));
    }

    [Theory]
    [InlineData(DiskImageFormatIds.Ibm160, 320)]
    [InlineData(DiskImageFormatIds.Ibm180, 360)]
    [InlineData(DiskImageFormatIds.Ibm320, 640)]
    [InlineData(DiskImageFormatIds.Ibm360, 720)]
    public void LegacyLayoutsRequireAUniformBootSectorAndTheirDeclaredCapacity(string formatId, int sectors)
    {
        var boot = new byte[FatBootSectorLayout.SectorSize];
        Assert.False(Fat12LegacyLayoutCatalog.TryCreateLayout(formatId, sectors, boot, out _));
        boot[1] = 1;
        Assert.True(Fat12LegacyLayoutCatalog.TryCreateLayout(formatId, sectors, boot, out _));
        Assert.False(Fat12LegacyLayoutCatalog.TryCreateLayout(formatId, sectors - 1, boot, out _));
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
        var cyclic = new Fat12FileSystemReader().Read(CreateImage(Fat12Table.FirstDataCluster));
        Assert.Contains(cyclic.Warnings, warning => warning.Contains("cyclique", StringComparison.Ordinal));
        var outOfRange = new Fat12FileSystemReader().Read(CreateImage(0x700));
        Assert.Contains(outOfRange.Warnings, warning => warning.Contains("hors de la plage", StringComparison.Ordinal));
        var complete = CreateImage(Fat12Table.FirstEndOfChain);
        var missing = new SectorImage(complete.FormatId, complete.BlockSize, complete.Cylinders, complete.Heads, complete.SectorsPerTrack, complete.AvailableBlocks.Where(block => block.LogicalBlock != 12));
        var incomplete = new Fat12FileSystemReader().Read(missing);
        Assert.Contains(incomplete.Warnings, warning => warning.Contains("au lieu de", StringComparison.Ordinal));
        Assert.False(Assert.Single(incomplete.Entries).MetadataValid);
    }

    [Theory]
    [InlineData("validated_images/Atari/Atari ST/3.5 pouces - Atari TOS FAT12 - 720 Kio/seeds-of-evil-atari-st.st")]
    [InlineData("IBM PC/Bank Street Writer for IBM PC (1984) (5.25-160k) DISK01S1.IMG")]
    [InlineData("validated_images/MSX/MSX/3.5 pouces - MSX-DOS FAT12 - 720 Kio/seeds-of-evil-msx.dsk")]
    public async Task PublicExplorerReadsRealAtariIbmAndMsxFat12Images(string relativePath)
    {
        var path = Path.Combine(FindImageTestRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path), path);
        var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.True(explored.FileSystemRecognized, path);
        Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Fat12, explored.Volume.FileSystemId);
        Assert.NotEmpty(explored.Volume.Entries);
        Assert.All(explored.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
        Assert.InRange(explored.Volume.FreeBytes, 0, explored.Volume.Capacity);
    }

    [Fact]
    public void DirectoryCodecsCoverLabelsEntriesAndFatDateTime()
    {
        var root = new byte[FatDirectoryLayout.EntrySize * 4];
        "ROOTLABEL  "u8.CopyTo(root);
        root[FatDirectoryLayout.AttributesOffset] = (byte)FatDirectoryAttributes.VolumeLabel;
        root[FatDirectoryLayout.EntrySize] = FatDirectoryLayout.DeletedMarker;
        root[2 * FatDirectoryLayout.EntrySize] = (byte)'L';
        root[2 * FatDirectoryLayout.EntrySize + FatDirectoryLayout.AttributesOffset] = (byte)FatDirectoryLayout.LongFileName;
        Assert.Equal("ROOTLABEL", FatDirectoryEntryReader.ReadVolumeLabel(root));
        Assert.Equal("FILE.TXT", FatDirectoryEntryReader.DecodeName("FILE    TXT"u8));
        var boot = new byte[FatBootSectorLayout.ExtendedBootMinimumLength];
        "BOOTLABEL  "u8.CopyTo(boot.AsSpan(FatBootSectorLayout.VolumeLabelOffset));
        Assert.Equal("BOOTLABEL", FatDirectoryEntryReader.ReadBootVolumeLabel(boot));
        "NO NAME    "u8.CopyTo(boot.AsSpan(FatBootSectorLayout.VolumeLabelOffset));
        Assert.Equal(string.Empty, FatDirectoryEntryReader.ReadBootVolumeLabel(boot));
        Assert.Null(FatDateTime.Decode(0, 0));
    }

    [Fact]
    public void SectorRangePreservesThePositionOfAMissingMiddleSector()
    {
        var image = new SectorImage(DiskImageFormatIds.Ibm360, FatBootSectorLayout.SectorSize, 1, 1, 3, [new SectorBlock(0, new SectorAddress(0, 0, 1), Enumerable.Repeat((byte)0x11, FatBootSectorLayout.SectorSize).ToArray()), new SectorBlock(2, new SectorAddress(0, 0, 3), Enumerable.Repeat((byte)0x33, FatBootSectorLayout.SectorSize).ToArray())]);
        var warnings = new List<string>();
        var range = FatSectorReader.Read(image, 0, 3, warnings);
        Assert.False(range.IsValid);
        Assert.Equal(0x11, range.Bytes[0]);
        Assert.Equal(0, range.Bytes[FatBootSectorLayout.SectorSize]);
        Assert.Equal(0x33, range.Bytes[2 * FatBootSectorLayout.SectorSize]);
        Assert.Single(warnings);
    }

    [Fact]
    public void ClusterChainPreservesAMissingSectorAndMarksTheContentInvalid()
    {
        var complete = CreateImage(Fat12Table.FirstEndOfChain);
        var missing = new SectorImage(complete.FormatId, complete.BlockSize, complete.Cylinders, complete.Heads, complete.SectorsPerTrack, complete.AvailableBlocks.Where(block => block.LogicalBlock != 12));
        var warnings = new List<string>();
        var fat = FatSectorReader.Read(missing, 1, 2, warnings);
        var layout = new Fat12Layout(1, 2, 5, 7, 12, 1, 708);
        var chain = Fat12ClusterChainReader.Read(missing, fat, layout, Fat12Table.FirstDataCluster, warnings, "FILE.TXT");
        Assert.False(chain.IsValid);
        Assert.Equal(FatBootSectorLayout.SectorSize, chain.Content.Count);
        Assert.All(chain.Content, value => Assert.Equal(0, value));
    }

    [Fact]
    public void DirectoryDepthLimitAndInvalidFatAreReported()
    {
        var image = CreateImage(Fat12Table.FirstEndOfChain);
        var warnings = new List<string>();
        var empty = new FatSectorRange(new byte[FatBootSectorLayout.SectorSize], [true]);
        Assert.Empty(Fat12DirectoryReader.Read(image, empty, empty, new Fat12Layout(1, 2, 5, 7, 12, 1, 708), warnings, Fat12DirectoryReader.MaximumDepth + 1, "deep"));
        Assert.Contains(warnings, warning => warning.Contains("profondeur", StringComparison.Ordinal));
        var invalidFatBlocks = image.AvailableBlocks.Select(block => block.LogicalBlock == 1 ? block with { Data = Array.AsReadOnly(new byte[FatBootSectorLayout.SectorSize]) } : block);
        var invalidFat = new SectorImage(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, invalidFatBlocks);
        Assert.Throws<InvalidDataException>(() => new Fat12FileSystemReader().Read(invalidFat));
        var missingFat = new SectorImage(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks.Where(block => block.LogicalBlock != 2));
        Assert.Throws<InvalidDataException>(() => new Fat12FileSystemReader().Read(missingFat));
        var missingRoot = new SectorImage(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks.Where(block => block.LogicalBlock != 5));
        var rootVolume = new Fat12FileSystemReader().Read(missingRoot);
        Assert.Empty(rootVolume.Entries);
        Assert.Contains(rootVolume.Warnings, warning => warning.Contains("secteur FAT 5", StringComparison.Ordinal));
    }

    private static SectorImage CreateImage(int nextCluster)
    {
        const int blockCount = 720;
        var data = Enumerable.Range(0, blockCount).Select(_ => new byte[FatBootSectorLayout.SectorSize]).ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
        data[0][FatBootSectorLayout.SectorsPerClusterOffset] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBootSectorLayout.ReservedSectorCountOffset), 1);
        data[0][FatBootSectorLayout.FatCountOffset] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBootSectorLayout.RootEntryCountOffset), 112);
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBootSectorLayout.TotalSectors16Offset), blockCount);
        BinaryPrimitives.WriteUInt16LittleEndian(data[0].AsSpan(FatBootSectorLayout.SectorsPerFatOffset), 2);
        "NO NAME    "u8.CopyTo(data[0].AsSpan(FatBootSectorLayout.VolumeLabelOffset));
        data[1][0] = 0xf9;
        data[1][1] = 0xff;
        data[1][2] = 0xff;
        data[1][3] = (byte)(nextCluster & 0xff);
        data[1][4] = (byte)(nextCluster >> 8 & 0x0f);
        "FILE    TXT"u8.CopyTo(data[5]);
        data[5][FatDirectoryLayout.AttributesOffset] = (byte)(FatDirectoryAttributes.ReadOnly | FatDirectoryAttributes.Hidden | FatDirectoryAttributes.System | FatDirectoryAttributes.Archive);
        BinaryPrimitives.WriteUInt16LittleEndian(data[5].AsSpan(FatDirectoryLayout.FirstClusterOffset), Fat12Table.FirstDataCluster);
        BinaryPrimitives.WriteUInt32LittleEndian(data[5].AsSpan(FatDirectoryLayout.FileSizeOffset), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(data[5].AsSpan(FatDirectoryLayout.ModifiedDateOffset), (ushort)((2024 - 1980) << 9 | 6 << 5 | 15));
        BinaryPrimitives.WriteUInt16LittleEndian(data[5].AsSpan(FatDirectoryLayout.ModifiedTimeOffset), (ushort)(12 << 11 | 34 << 5 | 28));
        data[5][FatDirectoryLayout.EntrySize] = FatDirectoryLayout.DeletedMarker;
        data[5][2 * FatDirectoryLayout.EntrySize] = (byte)'L';
        data[5][2 * FatDirectoryLayout.EntrySize + FatDirectoryLayout.AttributesOffset] = (byte)FatDirectoryLayout.LongFileName;
        data[12][0] = 0x42;
        var blocks = data.Select((bytes, index) => new SectorBlock(index, new SectorAddress(0, 0, index), bytes));
        return new SectorImage(DiskImageFormatIds.Ibm360, FatBootSectorLayout.SectorSize, 40, 2, 9, blocks);
    }

    /// <summary>Recherche le dossier local non versionné contenant les images de validation.</summary>
    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }
}
