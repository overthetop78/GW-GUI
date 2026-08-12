using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Dec.Rt11;
using GWGUI.MediaEngine.FileSystems.Readers;
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
    }

    [Fact]
    public void Radix50AndDatesAreDecoded()
    {
        Assert.Equal("ABC", Rt11DirectoryReader.DecodeRadix50(Encode("ABC")));
        Assert.Equal(new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero), Rt11DirectoryReader.DecodeDate((ushort)((2024 - Rt11FileSystemLayout.EpochYear) << 9 | 6 << 5 | 15)));
        Assert.Null(Rt11DirectoryReader.DecodeDate((ushort)(13 << 5 | 1)));
    }

    [Fact]
    public void PublicReaderReadsSegmentsAndReportsMissingPairAndContent()
    {
        var complete = CreateImage(includeSecondSegment: true, includeContent: false);
        var volume = new Rt11FileSystemReader().Read(complete);
        Assert.Single(volume.Entries);
        Assert.Contains(volume.Warnings, warning => warning.Contains("incomplete", StringComparison.Ordinal));
        var missingPair = CreateImage(includeSecondSegment: false, includeContent: true);
        var missing = new Rt11FileSystemReader().Read(missingPair);
        Assert.Contains(missing.Warnings, warning => warning.Contains("block pair", StringComparison.Ordinal));
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

    private static ushort Encode(string value)
    {
        value = value.PadRight(3)[..3];
        return checked((ushort)(Rt11FileSystemLayout.Radix50.IndexOf(value[0]) * 1600 + Rt11FileSystemLayout.Radix50.IndexOf(value[1]) * 40 + Rt11FileSystemLayout.Radix50.IndexOf(value[2])));
    }
}
