using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Dec.Rt11;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class Rt11DefinitionsTests
{
    [Fact]
    public void StatusFlagsKeepTheirOnDiskValues()
    {
        Assert.Equal(0x0100, (int)Rt11DirectoryEntryStatus.Tentative);
        Assert.Equal(0x0200, (int)Rt11DirectoryEntryStatus.Empty);
        Assert.Equal(0x0400, (int)Rt11DirectoryEntryStatus.Permanent);
        Assert.Equal(0x0800, (int)Rt11DirectoryEntryStatus.EndOfSegment);
        Assert.Equal(0x8000, (int)Rt11DirectoryEntryStatus.Protected);
        Assert.True((Rt11DirectoryEntryStatus.Permanent | Rt11DirectoryEntryStatus.Protected).HasFlag(Rt11DirectoryEntryStatus.Protected));
        Assert.Equal(Rt11FileSystemLayout.TentativeFileDescription, Rt11FileSystemLayout.FileDescription(Rt11DirectoryEntryStatus.Tentative));
        Assert.Equal(Rt11FileSystemLayout.PermanentFileDescription, Rt11FileSystemLayout.FileDescription(Rt11DirectoryEntryStatus.Permanent));
    }

    [Fact]
    public void Radix50AndDatesAreDecoded()
    {
        Assert.Equal("ABC", Rt11Radix50.Decode(Encode("ABC")));
        Assert.Equal(new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Rt11Date.Decode((ushort)((2024 - Rt11Date.BaseYear) << Rt11Date.YearShift | 6 << Rt11Date.MonthShift | 15)));
        Assert.Null(Rt11Date.Decode((ushort)(13 << Rt11Date.MonthShift | 1)));
    }

    [Fact]
    public void PublicReaderReadsSegmentsAndReportsMissingPairAndContent()
    {
        var complete = CreateImage(includeSecondSegment: true, includeContent: false);
        var volume = new Rt11FileSystemReader().Read(complete);
        Assert.Single(volume.Entries);
        Assert.Contains(volume.Warnings, warning => warning.Contains("absents ou tronqués", StringComparison.Ordinal));
        var missingPair = CreateImage(includeSecondSegment: false, includeContent: true);
        var missing = new Rt11FileSystemReader().Read(missingPair);
        Assert.Contains(missing.Warnings, warning => warning.Contains("paire absente", StringComparison.Ordinal));
    }

    [Fact]
    public void DirectoryReportsEmptyCycleOutOfRangeAndInvalidEntrySize()
    {
        var empty = CreateDirectoryOnlyImage(0, 0, Rt11DirectoryEntryStatus.Empty, 2);
        var emptyResult = Rt11DirectoryReader.Read(empty, 6);
        Assert.Equal(2, emptyResult.FreeBlocks);
        Assert.True(emptyResult.IsValid);
        Assert.True(Rt11DirectoryReader.Read(CreateDirectoryOnlyImage(0, 2, Rt11DirectoryEntryStatus.EndOfSegment, 0), 6).IsValid);
        Assert.False(Rt11DirectoryReader.Read(CreateDirectoryOnlyImage(1, 0, Rt11DirectoryEntryStatus.EndOfSegment, 0), 6).IsValid);
        Assert.False(Rt11DirectoryReader.Read(CreateDirectoryOnlyImage(32, 0, Rt11DirectoryEntryStatus.EndOfSegment, 0), 6).IsValid);
        Assert.False(Rt11DirectoryReader.Read(CreateDirectoryOnlyImage(0, Rt11FileSystemLayout.MaximumEntrySize, Rt11DirectoryEntryStatus.EndOfSegment, 0), 6).IsValid);
    }

    [Fact]
    public void PositionalContentKeepsMissingMiddleBlock()
    {
        var image = new SectorImage(DiskImageFormatIds.DecRx02, 512, 1, 1, 10, [Block(5, Enumerable.Repeat((byte)0x11, 512).ToArray()), Block(7, Enumerable.Repeat((byte)0x33, 512).ToArray())]);
        var content = Rt11FileContentReader.Read(image, 5, 3);
        Assert.False(content.IsValid);
        Assert.Equal([6], content.MissingBlocks);
        Assert.Equal(0x11, content.Content[0]);
        Assert.All(content.Content.Skip(512).Take(512), value => Assert.Equal(0, value));
        Assert.Equal(0x33, content.Content[1024]);
    }

    private static SectorImage CreateImage(bool includeSecondSegment, bool includeContent)
    {
        var blocks = new List<SectorBlock>();
        var home = new byte[Rt11FileSystemLayout.BlockSize];
        BinaryPrimitives.WriteUInt16LittleEndian(home.AsSpan(Rt11FileSystemLayout.DirectoryBlockOffset), 6);
        "VOLUME"u8.CopyTo(home.AsSpan(Rt11FileSystemLayout.VolumeNameOffset));
        "DECRT11"u8.CopyTo(home.AsSpan(Rt11FileSystemLayout.SystemIdOffset));
        blocks.Add(Block(Rt11FileSystemLayout.HomeBlock, home));
        var first = new byte[Rt11FileSystemLayout.BlockSize * 2];
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(Rt11FileSystemLayout.NextSegmentOffset), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(Rt11FileSystemLayout.DataBlockOffset), 20);
        var offset = Rt11FileSystemLayout.EntriesOffset;
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(offset), (ushort)(Rt11DirectoryEntryStatus.Permanent | Rt11DirectoryEntryStatus.Protected));
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(offset + Rt11FileSystemLayout.NameOffset), Encode("FIL"));
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(offset + Rt11FileSystemLayout.NameOffset + 2), Encode("E  "));
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(offset + Rt11FileSystemLayout.ExtensionOffset), Encode("TXT"));
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(offset + Rt11FileSystemLayout.BlockLengthOffset), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(first.AsSpan(offset + Rt11FileSystemLayout.MinimumEntrySize), (ushort)Rt11DirectoryEntryStatus.EndOfSegment);
        blocks.Add(Block(6, first[..Rt11FileSystemLayout.BlockSize]));
        blocks.Add(Block(7, first[Rt11FileSystemLayout.BlockSize..]));
        if (includeSecondSegment)
        {
            var second = new byte[Rt11FileSystemLayout.BlockSize * 2];
            BinaryPrimitives.WriteUInt16LittleEndian(second.AsSpan(Rt11FileSystemLayout.EntriesOffset), (ushort)Rt11DirectoryEntryStatus.EndOfSegment);
            blocks.Add(Block(8, second[..Rt11FileSystemLayout.BlockSize]));
            blocks.Add(Block(9, second[Rt11FileSystemLayout.BlockSize..]));
        }
        if (includeContent) blocks.Add(Block(20, new byte[Rt11FileSystemLayout.BlockSize]));
        return new SectorImage(DiskImageFormatIds.DecRx02, Rt11FileSystemLayout.BlockSize, 1, 1, 1001, blocks);
    }

    private static SectorBlock Block(int logical, byte[] data) => new(logical, new(0, 0, logical), data);

    private static SectorImage CreateDirectoryOnlyImage(ushort nextSegment, ushort extraBytes, Rt11DirectoryEntryStatus status, ushort blockLength)
    {
        var pair = new byte[Rt11FileSystemLayout.BlockSize * Rt11FileSystemLayout.SegmentBlockCount];
        BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(Rt11FileSystemLayout.NextSegmentOffset), nextSegment);
        BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(Rt11FileSystemLayout.ExtraBytesOffset), extraBytes);
        BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(Rt11FileSystemLayout.DataBlockOffset), 20);
        BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(Rt11FileSystemLayout.EntriesOffset), (ushort)status);
        BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(Rt11FileSystemLayout.EntriesOffset + Rt11FileSystemLayout.BlockLengthOffset), blockLength);
        if (!status.HasFlag(Rt11DirectoryEntryStatus.EndOfSegment)) BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(Rt11FileSystemLayout.EntriesOffset + Rt11FileSystemLayout.MinimumEntrySize + extraBytes), (ushort)Rt11DirectoryEntryStatus.EndOfSegment);
        return new SectorImage(DiskImageFormatIds.DecRx02, 512, 1, 1, 30, [Block(6, pair[..512]), Block(7, pair[512..])]);
    }

    private static ushort Encode(string value)
    {
        value = value.PadRight(3)[..3];
        return checked((ushort)(Rt11Radix50.Alphabet.IndexOf(value[0]) * Rt11Radix50.FirstDivisor + Rt11Radix50.Alphabet.IndexOf(value[1]) * Rt11Radix50.SecondDivisor + Rt11Radix50.Alphabet.IndexOf(value[2])));
    }
}
