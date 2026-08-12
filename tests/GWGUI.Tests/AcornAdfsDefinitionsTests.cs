using System.IO;
using GWGUI.MediaEngine.FileSystems.Acorn;
using GWGUI.MediaEngine.FileSystems.Acorn.Adfs;
using GWGUI.MediaEngine.FileSystems.Acorn.FileCore;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.Tests;

public sealed class AcornAdfsDefinitionsTests
{
    [Fact]
    public void UInt24ValidatesAndReadsItsRange()
    {
        Assert.Equal(0x563412, LittleEndianUInt24.Read([0x12, 0x34, 0x56], 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => LittleEndianUInt24.Read([1, 2], 0));
    }

    [Fact]
    public void NameCodecHandlesSevenBitsAndTerminators() => Assert.Equal("ABC", AcornAdfsNameCodec.Decode([(byte)'A', (byte)'B', (byte)(0x80 | 'C'), 0x0d, (byte)'D']));

    [Fact]
    public void OldMapReadsInterleavedNameAndCapsFreeSpace()
    {
        var map = new byte[1024];
        "ACEGI"u8.CopyTo(map.AsSpan(247));
        "BDFHJ"u8.CopyTo(map.AsSpan(502));
        map[256] = 10;
        var resolver = new AcornFileCoreOldMap(map, 2048);
        Assert.Equal("ABCDEFGHIJ", resolver.VolumeName);
        Assert.Equal(2048, resolver.FreeBytes);
        Assert.True(resolver.TryResolveByteOffset(4, 0, out var offset));
        Assert.Equal(1024, offset);
    }

    [Fact]
    public void RiscOsTimeDistinguishesTimestampAndOrdinaryLoadAddress()
    {
        Assert.Null(AcornFileSystemTime.Decode(0, 0));
        Assert.Equal(AcornFileSystemTime.Epoch, AcornFileSystemTime.Decode(AcornFileSystemTime.TimestampMarker, 0));
    }

    [Fact]
    public void FileReaderDoesNotReturnArtificialZeroPaddingWhenABlockIsMissing()
    {
        var image = new SectorImage("test", AcornAdfsLayout.BlockSize, 1, 1, 3, [new(1, new(0, 0, 1), new byte[AcornAdfsLayout.BlockSize])]);
        var resolver = new AcornFileCoreOldMap(new byte[1024], image.Capacity);
        var warnings = new List<string>();
        var valid = true;
        var content = AcornAdfsFileReader.Read(image, 8, 1, resolver, "FILE", warnings, ref valid);
        Assert.Null(content);
        Assert.False(valid);
        Assert.NotEmpty(warnings);
    }

    [Fact]
    public void FileReaderHandlesEmptyMultiBlockAndInvalidOffsets()
    {
        var blocks = new[]
        {
            new SectorBlock(0, new(0, 0, 0), Enumerable.Repeat((byte)1, AcornAdfsLayout.BlockSize).ToArray()),
            new SectorBlock(1, new(0, 0, 1), Enumerable.Repeat((byte)2, AcornAdfsLayout.BlockSize).ToArray())
        };
        var image = new SectorImage("test", AcornAdfsLayout.BlockSize, 1, 1, 2, blocks);
        var resolver = new LinearResolver(image.Capacity);
        var warnings = new List<string>();
        var valid = true;
        Assert.Empty(AcornAdfsFileReader.Read(image, 1, 0, resolver, "EMPTY", warnings, ref valid)!);
        var content = AcornAdfsFileReader.Read(image, 1, AcornAdfsLayout.BlockSize + 1, resolver, "MULTI", warnings, ref valid);
        Assert.Equal(AcornAdfsLayout.BlockSize + 1, content!.Count);
        Assert.Equal(2, content[^1]);
        valid = true;
        Assert.Null(AcornAdfsFileReader.Read(image, 99, 1, resolver, "BAD", warnings, ref valid));
        Assert.False(valid);
    }

    [Fact]
    public void DirectoryReaderReportsTheDepthLimitBeforeReadingBlocks()
    {
        var warnings = new List<string>();
        var image = new SectorImage("test", AcornAdfsLayout.BlockSize, 1, 1, 1, []);
        var result = AcornAdfsDirectoryReader.Read(image, 1, new LinearResolver(image.Capacity), new HashSet<int>(), warnings, AcornAdfsLayout.MaximumDepth + 1);
        Assert.Empty(result.Children);
        Assert.Contains(warnings, warning => warning.Contains("depth", StringComparison.Ordinal));
    }

    [Fact]
    public async Task PublicReaderExploresTheKnownAdfsImage()
    {
        var path = Path.Combine(FindImageTestRoot(), "adfs_ArchimedesWorld_199211.adf");
        var bytes = await File.ReadAllBytesAsync(path);
        Assert.Equal(AcornAdfsLayout.ImageBlockCount * AcornAdfsLayout.BlockSize, bytes.Length);
        var blocks = Enumerable.Range(0, AcornAdfsLayout.ImageBlockCount).Select(index => new SectorBlock(index, new(0, 0, index), bytes.AsSpan(index * AcornAdfsLayout.BlockSize, AcornAdfsLayout.BlockSize).ToArray()));
        var image = new SectorImage(DiskImageFormatIds.AcornAdfs800, AcornAdfsLayout.BlockSize, 80, 2, 5, blocks);
        var reader = new AcornAdfsFileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.False(string.IsNullOrWhiteSpace(volume.Name));
        Assert.NotEmpty(volume.Entries);
    }

    [Fact]
    public void ReaderRejectsAnUnexpectedGeometry()
    {
        var image = new SectorImage(DiskImageFormatIds.AcornAdfs800, AcornAdfsLayout.BlockSize, 1, 1, 799, []);
        Assert.False(new AcornAdfsFileSystemReader().CanRead(image));
    }

    [Theory]
    [InlineData("Hugo", true)]
    [InlineData("Nick", true)]
    [InlineData("Bad!", false)]
    public void OldMapReaderValidatesDirectorySignatures(string signature, bool expected)
    {
        var image = CreateOldMapImage(signature, signature, 7, 7, includeCycle: false);
        Assert.Equal(expected, new AcornAdfsFileSystemReader().CanRead(image));
    }

    [Fact]
    public void OldMapReaderRejectsMismatchedFooterAndSequence()
    {
        Assert.False(new AcornAdfsFileSystemReader().CanRead(CreateOldMapImage("Hugo", "Nick", 7, 7, false)));
        Assert.False(new AcornAdfsFileSystemReader().CanRead(CreateOldMapImage("Hugo", "Hugo", 7, 8, false)));
    }

    [Fact]
    public void OldMapReaderReportsARecursiveReference()
    {
        var volume = new AcornAdfsFileSystemReader().Read(CreateOldMapImage("Hugo", "Hugo", 7, 7, true));
        Assert.Contains(volume.Warnings, warning => warning.Contains("referenced more than once", StringComparison.Ordinal));
    }

    private static SectorImage CreateOldMapImage(string header, string footer, byte firstSequence, byte lastSequence, bool includeCycle)
    {
        var data = new byte[AcornAdfsLayout.ImageBlockCount * AcornAdfsLayout.BlockSize];
        var directoryOffset = AcornAdfsLayout.FileCoreUnitSize * 4;
        data[directoryOffset] = firstSequence;
        System.Text.Encoding.ASCII.GetBytes(header).CopyTo(data, directoryOffset + AcornAdfsLayout.HeaderSignatureOffset);
        System.Text.Encoding.ASCII.GetBytes(footer).CopyTo(data, directoryOffset + AcornAdfsLayout.FooterSignatureOffset);
        data[directoryOffset + AcornAdfsLayout.TailSequenceOffset] = lastSequence;
        "ROOT"u8.CopyTo(data.AsSpan(directoryOffset + AcornAdfsLayout.DirectoryNameOffset));
        if (includeCycle)
        {
            var entry = directoryOffset + AcornAdfsLayout.EntriesOffset;
            "LOOP"u8.CopyTo(data.AsSpan(entry));
            data[entry + AcornAdfsLayout.EntryIndirectAddressOffset] = 4;
            data[entry + AcornAdfsLayout.EntryAttributesOffset] = AcornAdfsLayout.DirectoryAttribute;
        }
        var blocks = Enumerable.Range(0, AcornAdfsLayout.ImageBlockCount).Select(index => new SectorBlock(index, new(0, 0, index), data.AsSpan(index * AcornAdfsLayout.BlockSize, AcornAdfsLayout.BlockSize).ToArray()));
        return new SectorImage(DiskImageFormatIds.AcornAdfs800, AcornAdfsLayout.BlockSize, 80, 2, 5, blocks);
    }

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }

    private sealed class LinearResolver(long capacity) : IFileCoreAddressResolver
    {
        public int RootAddress => 1;
        public string VolumeName => string.Empty;
        public long FreeBytes => 0;
        public bool TryResolveByteOffset(int indirectAddress, long objectByteOffset, out long physicalByteOffset)
        {
            physicalByteOffset = (indirectAddress - 1L) * AcornAdfsLayout.BlockSize + objectByteOffset;
            return indirectAddress > 0 && physicalByteOffset >= 0 && physicalByteOffset < capacity;
        }
    }
}
