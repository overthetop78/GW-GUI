using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.Migration;
using System.IO;

namespace GWGUI.Tests;

public sealed class AmigaDosVolumeWriterTests
{
    [Theory]
    [InlineData(AmigaDosVariant.Ofs)]
    [InlineData(AmigaDosVariant.Ffs)]
    public void CreatedVolumeRoundTripsDirectoriesFilesMetadataBitmapAndChecksums(AmigaDosVariant variant)
    {
        var content = Enumerable.Range(0, 50_000).Select(index => (byte)(index * 31)).ToArray();
        var modified = DateTimeOffset.Parse("1992-04-10T19:28:00+00:00");
        var file = new MigrationEntry("Docs/Data.bin", "Data.bin", FileSystemEntryKind.File, content, modified, "comment", 3, true, []);
        var directory = new MigrationEntry("Docs", "Docs", FileSystemEntryKind.Directory, null, modified, string.Empty, 0, true, [file]);
        var plan = new MigrationPlan("source.fs", variant.FileSystemId(), "TARGET", [directory]);

        var image = new AmigaDosVolumeWriter().Create(plan, variant);
        var volume = new AmigaDosFileSystemReader().Read(image);

        Assert.Equal("TARGET", volume.Name);
        Assert.Equal(variant.FileSystemId(), volume.FileSystemId);
        Assert.True(volume.FreeBytes > 0);
        var readDirectory = Assert.Single(volume.Entries);
        var readFile = Assert.Single(readDirectory.Children);
        Assert.Equal("Data.bin", readFile.Name);
        Assert.Equal(content, readFile.Content);
        Assert.Equal("comment", readFile.Comment);
        Assert.Equal(3u, readFile.RawAttributes);
        Assert.True(readFile.MetadataValid);
        Assert.Empty(volume.Warnings);
    }

    [Fact]
    public void WriterRejectsUnsupportedVariantsAndNamesInsteadOfTruncatingThem()
    {
        var invalid = new MigrationEntry("too-long", new string('x', 31), FileSystemEntryKind.File, [], null, string.Empty, 0, true, []);
        var plan = new MigrationPlan("source.fs", DiskImageFormatIds.AmigaDos, "TARGET", [invalid]);

        Assert.Throws<InvalidDataException>(() => new AmigaDosVolumeWriter().Create(plan, AmigaDosVariant.FfsDirectoryCache));
        Assert.Throws<InvalidDataException>(() => new AmigaDosVolumeWriter().Create(plan, AmigaDosVariant.Ffs));
    }
}
