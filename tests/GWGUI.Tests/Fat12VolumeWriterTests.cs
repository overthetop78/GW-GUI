using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Migration;
using System.IO;

namespace GWGUI.Tests;

public sealed class Fat12VolumeWriterTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AtariSt720)]
    [InlineData(DiskImageFormatIds.Ibm720)]
    [InlineData(DiskImageFormatIds.Msx2Dd)]
    public void CreatedVolumeRoundTripsDirectoriesFilesFatCopiesAndFreeSpace(string formatId)
    {
        var content = Enumerable.Range(0, 5_000).Select(index => (byte)(index * 29)).ToArray();
        var modified = DateTimeOffset.Parse("1992-04-10T19:28:00+00:00");
        var file = new MigrationEntry("DOCS/README.TXT", "README.TXT", FileSystemEntryKind.File, content, modified, string.Empty, 0, true, []);
        var directory = new MigrationEntry("DOCS", "DOCS", FileSystemEntryKind.Directory, null, modified, string.Empty, 0, true, [file]);
        var plan = new MigrationPlan("source.fs", FileSystemIds.Fat12, "TARGET", [directory]);

        var image = new Fat12VolumeWriter().Create(plan, formatId);
        var volume = new Fat12FileSystemReader().Read(image);

        Assert.Equal("TARGET", volume.Name);
        Assert.True(volume.FreeBytes > 0);
        var readFile = Assert.Single(Assert.Single(volume.Entries).Children);
        Assert.Equal("README.TXT", readFile.Name);
        Assert.Equal(content, readFile.Content);
        Assert.True(readFile.MetadataValid);
        Assert.Empty(volume.Warnings);
        Assert.Equal(image.GetBlock(1).ToArray(), image.GetBlock(4).ToArray());
    }

    [Fact]
    public void WriterRejectsLongNamesAndMissingContent()
    {
        var longName = new MigrationEntry("LONGFILENAME.TXT", "LONGFILENAME.TXT", FileSystemEntryKind.File, [], null, string.Empty, 0, true, []);
        var missing = new MigrationEntry("MISSING.BIN", "MISSING.BIN", FileSystemEntryKind.File, null, null, string.Empty, 0, true, []);

        Assert.Throws<InvalidDataException>(() => new Fat12VolumeWriter().Create(new("source.fs", FileSystemIds.Fat12, "TARGET", [longName]), DiskImageFormatIds.Ibm720));
        Assert.Throws<InvalidDataException>(() => new Fat12VolumeWriter().Create(new("source.fs", FileSystemIds.Fat12, "TARGET", [missing]), DiskImageFormatIds.Ibm720));
    }
}
