using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Ucsd;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class UcsdDefinitionsTests
{
    [Theory]
    [InlineData(UcsdFileSystemLayout.ShortDirectoryEnd, UcsdByteOrder.LittleEndian)]
    [InlineData(UcsdFileSystemLayout.LongDirectoryEnd, UcsdByteOrder.LittleEndian)]
    public void DetectsLittleEndianDirectoryEnds(int end, UcsdByteOrder expected)
    {
        var bytes = new byte[4];
        bytes[2] = (byte)end;
        Assert.True(UcsdDirectoryHeaderReader.TryDetectByteOrder(bytes, out var result));
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(UcsdFileSystemLayout.ShortDirectoryEnd)]
    [InlineData(UcsdFileSystemLayout.LongDirectoryEnd)]
    public void DetectsBigEndianDirectoryEnds(int end)
    {
        var bytes = new byte[4];
        bytes[3] = (byte)end;
        Assert.True(UcsdDirectoryHeaderReader.TryDetectByteOrder(bytes, out var result));
        Assert.Equal(UcsdByteOrder.BigEndian, result);
    }

    [Theory]
    [InlineData(UcsdFileKind.Untyped, "UCSD untyped file")]
    [InlineData(UcsdFileKind.ExternalDisk, "UCSD external disk file")]
    [InlineData(UcsdFileKind.Code, "UCSD code file")]
    [InlineData(UcsdFileKind.Text, "UCSD text file")]
    [InlineData(UcsdFileKind.Info, "UCSD info file")]
    [InlineData(UcsdFileKind.Data, "UCSD data file")]
    [InlineData(UcsdFileKind.Graphics, "UCSD graphics file")]
    [InlineData(UcsdFileKind.Photo, "UCSD photo file")]
    [InlineData(UcsdFileKind.SecureDirectory, "UCSD secure directory")]
    public void EveryFileKindHasAName(UcsdFileKind kind, string expected) => Assert.Equal(expected, UcsdFileKindNames.Get(kind));

    [Fact]
    public void UnknownByteOrderAndInvalidNamesAreReported()
    {
        Assert.False(UcsdDirectoryHeaderReader.TryDetectByteOrder([1, 2, 3, 4], out _));
        Assert.Contains("3", UcsdFileSystemExceptions.InvalidName(3, string.Empty));
        Assert.Contains("[9, 2)", UcsdFileSystemExceptions.InvalidRange(3, string.Empty, 9, 2, 20));
    }

    [Theory]
    [InlineData(UcsdByteOrder.LittleEndian, UcsdFileSystemLayout.ShortDirectoryEnd)]
    [InlineData(UcsdByteOrder.BigEndian, UcsdFileSystemLayout.LongDirectoryEnd)]
    public void PublicReaderReadsBothOrdersAndDirectoryLengths(UcsdByteOrder order, int endDirectory)
    {
        var image = CreateImage(order, endDirectory, includeData: true);
        var volume = new UcsdFileSystemReader().Read(image);
        Assert.Equal("VOL", volume.Name);
        Assert.Equal(new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Assert.Single(volume.Entries).Modified);
    }

    [Fact]
    public void PublicReaderReportsAnIncompleteFileRange()
    {
        var volume = new UcsdFileSystemReader().Read(CreateImage(UcsdByteOrder.LittleEndian, UcsdFileSystemLayout.ShortDirectoryEnd, includeData: false));
        Assert.Contains(volume.Warnings, warning => warning.Contains("blocs", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("1234567", 7, true)]
    [InlineData("12345678", 7, false)]
    [InlineData("123456789012345", 15, true)]
    [InlineData("1234567890123456", 15, false)]
    [InlineData("", 15, false)]
    public void ValidatesPascalNameLimits(string value, int maximumLength, bool expected)
    {
        var field = new byte[value.Length + 1];
        field[0] = (byte)value.Length;
        System.Text.Encoding.ASCII.GetBytes(value).CopyTo(field, 1);
        Assert.Equal(expected, UcsdName.Decode(field, maximumLength).Length > 0);
    }

    [Fact]
    public void RejectsNonPrintablePascalName() => Assert.Empty(UcsdName.Decode([1, 0x1f], UcsdFileSystemLayout.MaximumFileNameLength));

    [Theory]
    [InlineData(69, 2069)]
    [InlineData(70, 1970)]
    public void DecodesBothDateCenturies(int encodedYear, int expectedYear)
    {
        var value = (ushort)(encodedYear << UcsdDate.YearShift | 1 << UcsdDate.MonthShift | 1);
        Assert.Equal(expectedYear, UcsdDate.Decode(value)?.Year);
    }

    [Fact]
    public void PreservesPositionsWhenAMiddleDataBlockIsMissing()
    {
        var blocks = new[]
        {
            new SectorBlock(12, new(0, 0, 12), Enumerable.Repeat((byte)0x12, UcsdFileSystemLayout.BlockSize).ToArray()),
            new SectorBlock(14, new(0, 0, 14), Enumerable.Repeat((byte)0x14, UcsdFileSystemLayout.BlockSize).ToArray())
        };
        var image = new SectorImage(DiskImageFormatIds.UcsdIbmMfm, UcsdFileSystemLayout.BlockSize, 1, 1, 20, blocks);
        var warnings = new List<string>();
        var content = UcsdFileContentReader.Read(image, 12, 15, 1, "TEST", warnings);
        Assert.False(content.IsValid);
        Assert.Equal(13, Assert.Single(content.MissingBlocks));
        Assert.Equal(0x12, content.Content[0]);
        Assert.Equal(0, content.Content[UcsdFileSystemLayout.BlockSize]);
        Assert.Equal(0x14, content.Content[2 * UcsdFileSystemLayout.BlockSize]);
    }

    [Theory]
    [InlineData(0, 512)]
    [InlineData(17, 17)]
    public void AppliesLastBlockByteConvention(int lastBytes, int expectedSize)
    {
        var image = new SectorImage(DiskImageFormatIds.UcsdIbmMfm, UcsdFileSystemLayout.BlockSize, 1, 1, 20, [new SectorBlock(12, new(0, 0, 12), new byte[UcsdFileSystemLayout.BlockSize])]);
        var result = UcsdFileContentReader.Read(image, 12, 13, lastBytes, "TEST", []);
        Assert.True(result.IsValid);
        Assert.Equal(expectedSize, result.Size);
    }

    [Fact]
    public void RejectsLastBlockByteCountAboveBlockSize()
    {
        var image = new SectorImage(DiskImageFormatIds.UcsdIbmMfm, UcsdFileSystemLayout.BlockSize, 1, 1, 20, []);
        var warnings = new List<string>();
        var result = UcsdFileContentReader.Read(image, 12, 13, UcsdFileSystemLayout.BlockSize + 1, "TEST", warnings);
        Assert.False(result.IsValid);
        Assert.Single(warnings);
    }

    [Fact]
    public void UnknownFileKindUsesUntypedName() => Assert.Equal(UcsdFileKindNames.Get(UcsdFileKind.Untyped), UcsdFileKindNames.Get((UcsdFileKind)15));

    [Theory]
    [InlineData(UcsdFileSystemLayout.ShortDirectoryEnd, 3)]
    [InlineData(UcsdFileSystemLayout.LongDirectoryEnd, 8)]
    public void ReportsMissingDirectoryBlockWithoutComputingFreeSpace(int endDirectory, int missingBlock)
    {
        var source = CreateImage(UcsdByteOrder.LittleEndian, endDirectory, includeData: true);
        var image = new SectorImage(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, source.AvailableBlocks.Where(block => block.LogicalBlock != missingBlock), logicalBlockCount: source.BlockCount);
        var volume = new UcsdFileSystemReader().Read(image);
        Assert.Equal(0, volume.FreeBytes);
        Assert.Contains(volume.Warnings, warning => warning.Contains(missingBlock.ToString(), StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(12, 12)]
    [InlineData(13, 12)]
    [InlineData(12, 21)]
    [InlineData(5, 12)]
    public void RejectsInvalidFileRanges(int firstBlock, int lastBlock)
    {
        var image = MutateDirectory(CreateImage(UcsdByteOrder.LittleEndian, UcsdFileSystemLayout.ShortDirectoryEnd, includeData: true), directory =>
        {
            Write(directory, UcsdFileSystemLayout.EntrySize + UcsdFileSystemLayout.EntryFirstBlockOffset, (ushort)firstBlock, UcsdByteOrder.LittleEndian);
            Write(directory, UcsdFileSystemLayout.EntrySize + UcsdFileSystemLayout.EntryLastBlockOffset, (ushort)lastBlock, UcsdByteOrder.LittleEndian);
        });
        var volume = new UcsdFileSystemReader().Read(image);
        Assert.Empty(volume.Entries);
        Assert.Contains(volume.Warnings, warning => warning.Contains("plage invalide", StringComparison.Ordinal));
    }

    [Fact]
    public void ReportsOverlappingFileRangesOnlyOnceInUsedSpace()
    {
        var image = MutateDirectory(CreateImage(UcsdByteOrder.LittleEndian, UcsdFileSystemLayout.ShortDirectoryEnd, includeData: true), directory =>
        {
            Write(directory, UcsdFileSystemLayout.FileCountOffset, 2, UcsdByteOrder.LittleEndian);
            var second = 2 * UcsdFileSystemLayout.EntrySize;
            Write(directory, second + UcsdFileSystemLayout.EntryFirstBlockOffset, 12, UcsdByteOrder.LittleEndian);
            Write(directory, second + UcsdFileSystemLayout.EntryLastBlockOffset, 13, UcsdByteOrder.LittleEndian);
            directory[second + UcsdFileSystemLayout.EntryNameOffset] = 3;
            "TWO"u8.CopyTo(directory.AsSpan(second + UcsdFileSystemLayout.EntryNameOffset + 1));
        });
        var volume = new UcsdFileSystemReader().Read(image);
        Assert.Contains(volume.Warnings, warning => warning.Contains("chevauche", StringComparison.Ordinal));
        Assert.Equal(2, volume.Entries.Count);
    }

    private static SectorImage CreateImage(UcsdByteOrder order, int endDirectory, bool includeData)
    {
        var directory = new byte[UcsdFileSystemLayout.LongDirectoryBlockCount * UcsdFileSystemLayout.BlockSize];
        Write(directory, UcsdFileSystemLayout.DirectoryEndOffset, (ushort)endDirectory, order);
        directory[UcsdFileSystemLayout.VolumeNameOffset] = 3;
        "VOL"u8.CopyTo(directory.AsSpan(UcsdFileSystemLayout.VolumeNameOffset + 1));
        Write(directory, UcsdFileSystemLayout.TotalBlocksOffset, 20, order);
        Write(directory, UcsdFileSystemLayout.FileCountOffset, 1, order);
        var entry = UcsdFileSystemLayout.EntrySize;
        Write(directory, entry + UcsdFileSystemLayout.EntryFirstBlockOffset, 12, order);
        Write(directory, entry + UcsdFileSystemLayout.EntryLastBlockOffset, 13, order);
        Write(directory, entry + UcsdFileSystemLayout.EntryKindOffset, (ushort)UcsdFileKind.Text, order);
        directory[entry + UcsdFileSystemLayout.EntryNameOffset] = 4;
        "TEST"u8.CopyTo(directory.AsSpan(entry + UcsdFileSystemLayout.EntryNameOffset + 1));
        Write(directory, entry + UcsdFileSystemLayout.EntryLastBlockBytesOffset, 1, order);
        Write(directory, entry + UcsdFileSystemLayout.EntryDateOffset, (ushort)(24 << 9 | 6 << 5 | 15), order);
        var blocks = new List<SectorBlock>();
        var count = endDirectory - UcsdFileSystemLayout.DirectoryBlock;
        for (var index = 0; index < count; index++) blocks.Add(new(UcsdFileSystemLayout.DirectoryBlock + index, new(0, 0, index), directory.AsSpan(index * UcsdFileSystemLayout.BlockSize, UcsdFileSystemLayout.BlockSize).ToArray()));
        if (includeData) blocks.Add(new(12, new(0, 0, 12), new byte[UcsdFileSystemLayout.BlockSize]));
        return new SectorImage(DiskImageFormatIds.UcsdIbmMfm, UcsdFileSystemLayout.BlockSize, 1, 1, 20, blocks);
    }

    private static void Write(Span<byte> data, int offset, ushort value, UcsdByteOrder order)
    {
        if (order == UcsdByteOrder.LittleEndian) System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data[offset..], value);
        else System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(data[offset..], value);
    }

    private static SectorImage MutateDirectory(SectorImage source, Action<byte[]> mutation)
    {
        var directory = source.AvailableBlocks.Single(block => block.LogicalBlock == UcsdFileSystemLayout.DirectoryBlock).Data.ToArray();
        mutation(directory);
        var blocks = source.AvailableBlocks.Select(block => block.LogicalBlock == UcsdFileSystemLayout.DirectoryBlock ? block with { Data = directory } : block);
        return new(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, blocks, logicalBlockCount: source.BlockCount);
    }
}
