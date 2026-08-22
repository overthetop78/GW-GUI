using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.Primitives;
using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class AmigaDosDefinitionsTests
{
    [Fact]
    public void NameCodecReadsOrdinaryLongAndBoundedStrings()
    {
        var block = new byte[AmigaDosLayout.BlockSize];
        block[AmigaDosLayout.OrdinaryNameOffset] = 3;
        "ABC"u8.CopyTo(block.AsSpan(AmigaDosLayout.OrdinaryNameOffset + 1));
        Assert.Equal("ABC", AmigaDosNameCodec.ReadEntryName(block, AmigaDosVariant.Ofs));
        block[AmigaDosLayout.OrdinaryNameOffset] = 0;
        block[AmigaDosLayout.LongNameOffset] = 4;
        "LONG"u8.CopyTo(block.AsSpan(AmigaDosLayout.LongNameOffset + 1));
        Assert.Equal("LONG", AmigaDosNameCodec.ReadEntryName(block, AmigaDosVariant.OfsLongNames));
        Assert.Equal("LO", AmigaDosNameCodec.Read(block, AmigaDosLayout.LongNameOffset, 2));
        Assert.Equal(string.Empty, AmigaDosNameCodec.Read(block, -1, 2));
    }

    [Fact]
    public void TimeReadsValidAndRejectsEveryInvalidComponent()
    {
        var data = new byte[12];
        BinaryPrimitives.WriteInt32BigEndian(data, 1);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4), 2);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(8), 3);
        Assert.NotNull(AmigaDosTime.Read(data, 0));
        BinaryPrimitives.WriteInt32BigEndian(data, -1);
        Assert.Null(AmigaDosTime.Read(data, 0));
        BinaryPrimitives.WriteInt32BigEndian(data, 0);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4), AmigaDosLayout.MinutesPerDay);
        Assert.Null(AmigaDosTime.Read(data, 0));
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(4), 0);
        BinaryPrimitives.WriteInt32BigEndian(data.AsSpan(8), 60 * AmigaDosLayout.TicksPerSecond);
        Assert.Null(AmigaDosTime.Read(data, 0));
        Assert.Null(AmigaDosTime.Read(data.AsSpan(0, 8), 0));
    }

    [Fact]
    public void EntryTypesMapToTheFourCommonKinds()
    {
        Assert.Equal(FileSystemEntryKind.Directory, AmigaDosEntryType.Directory.ToCommonKind());
        Assert.Equal(FileSystemEntryKind.File, AmigaDosEntryType.File.ToCommonKind());
        Assert.Equal(FileSystemEntryKind.Link, AmigaDosEntryType.HardLink.ToCommonKind());
        Assert.Equal(FileSystemEntryKind.Link, AmigaDosEntryType.DirectoryLink.ToCommonKind());
        Assert.Equal(FileSystemEntryKind.Link, AmigaDosEntryType.FileLink.ToCommonKind());
        Assert.Equal(FileSystemEntryKind.Unknown, AmigaDosEntryTypeExtensions.FromRaw(99).ToCommonKind());
    }

    [Fact]
    public void BigEndianPrimitiveValidatesItsRange()
    {
        Assert.Equal(0x01020304, BigEndianInt32.Read([1, 2, 3, 4], 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => BigEndianInt32.Read([1, 2, 3], 0));
    }

    [Fact]
    public async Task ReaderExploresKnownAmigaDosImage()
    {
        var path = FindKnownAdf();
        var image = await new AdfReader().ReadAsync(path);
        var volume = new AmigaDosFileSystemReader().Read(image);
        Assert.False(string.IsNullOrWhiteSpace(volume.Name));
        Assert.NotEmpty(volume.Entries);
        Assert.Equal(AmigaDosVariant.Ofs.FileSystemId(), volume.FileSystemId);
        Assert.All(volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
    }

    private static string FindKnownAdf()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var root = Path.Combine(directory.FullName, "image_test");
            if (!Directory.Exists(root)) continue;
            var path = Directory.EnumerateFiles(root, "seeds-of-evil-amiga.adf", SearchOption.AllDirectories).FirstOrDefault();
            if (path is not null) return path;
        }
        throw new FileNotFoundException("L'image AmigaDOS de test est introuvable.");
    }
}
