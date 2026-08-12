using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.FileSystems.Ucsd;
using GWGUI.MediaEngine.Definitions;
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
        var result = UcsdFileSystemReader.DetectByteOrder(bytes);
        Assert.True(result.Success);
        Assert.Equal(expected, result.ByteOrder);
    }

    [Theory]
    [InlineData(UcsdFileSystemLayout.ShortDirectoryEnd)]
    [InlineData(UcsdFileSystemLayout.LongDirectoryEnd)]
    public void DetectsBigEndianDirectoryEnds(int end)
    {
        var bytes = new byte[4];
        bytes[3] = (byte)end;
        Assert.Equal(UcsdByteOrder.BigEndian, UcsdFileSystemReader.DetectByteOrder(bytes).ByteOrder);
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
        Assert.False(UcsdFileSystemReader.DetectByteOrder([1, 2, 3, 4]).Success);
        Assert.Contains("01020304", UcsdFileSystemExceptions.UnknownByteOrder([1, 2, 3, 4]).Message);
        Assert.Contains("entry 3", UcsdFileSystemExceptions.InvalidEntry(3, string.Empty, 9, 2));
        Assert.Contains("incomplete", UcsdFileSystemExceptions.IncompleteRange(5, 2, 512));
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
        Assert.Contains(volume.Warnings, warning => warning.Contains("blocks", StringComparison.OrdinalIgnoreCase));
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
}
