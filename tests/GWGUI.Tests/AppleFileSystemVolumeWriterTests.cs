using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.Dos;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Sos;
using GWGUI.MediaEngine.Migration;

namespace GWGUI.Tests;

public sealed class AppleFileSystemVolumeWriterTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos113, 13)]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos140, 16)]
    public void AppleDosVolumeRoundTripsVtocCatalogListsAndContent(string formatId, int sectorsPerTrack)
    {
        var content = Enumerable.Range(0, 700).Select(index => (byte)(index * 17)).ToArray();
        var entry = new MigrationEntry("DEMO", "DEMO", FileSystemEntryKind.File, content, null, string.Empty, 0, true, []);
        var plan = new MigrationPlan("source.fs", FileSystemIds.AppleDos, "DOS-254", [entry]);

        var image = new AppleDosVolumeWriter().Create(plan, formatId);
        var volume = new AppleDosFileSystemReader().Read(image);

        Assert.Equal(sectorsPerTrack, image.SectorsPerTrack);
        Assert.Equal("DOS-254", volume.Name);
        Assert.Empty(volume.Warnings);
        var read = Assert.Single(volume.Entries);
        Assert.NotNull(read.Content);
        Assert.Equal("DEMO", read.Name);
        Assert.Equal(content, read.Content);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIProDos140, 280)]
    [InlineData(DiskImageFormatIds.AppleIIProDos800, 1600)]
    public void ProDosVolumeRoundTripsDirectoriesSeedlingSaplingAndTreeFiles(string formatId, int blocks)
    {
        var small = CreateFile("SMALL", 123);
        var medium = CreateFile("MEDIUM", 8_000);
        var large = CreateFile("LARGE", 100_000);
        var directory = new MigrationEntry("DOCS", "DOCS", FileSystemEntryKind.Directory, null, DateTimeOffset.Parse("1992-04-10T19:28:00Z"), string.Empty, 0, true, [small, medium]);
        var plan = new MigrationPlan("source.fs", FileSystemIds.ProDos, "TARGET", [directory, large]);

        var image = new ProDosVolumeWriter().Create(plan, formatId);
        var volume = new ProDosFileSystemReader().Read(image);

        Assert.Equal(blocks, image.BlockCount);
        Assert.Equal("TARGET", volume.Name);
        Assert.Empty(volume.Warnings);
        var readDirectory = Assert.Single(volume.Entries, entry => entry.Kind == FileSystemEntryKind.Directory);
        Assert.Equal(small.Content, Assert.Single(readDirectory.Children, entry => entry.Name == "SMALL").Content);
        Assert.Equal(medium.Content, Assert.Single(readDirectory.Children, entry => entry.Name == "MEDIUM").Content);
        Assert.Equal(large.Content, Assert.Single(volume.Entries, entry => entry.Name == "LARGE").Content);
    }

    [Fact]
    public void ProDos800VolumeRoundTripsTreeFile()
    {
        var large = CreateFile("TREEFILE", 300_000);
        var plan = new MigrationPlan("source.fs", FileSystemIds.ProDos, "TARGET", [large]);

        var volume = new ProDosFileSystemReader().Read(new ProDosVolumeWriter().Create(plan, DiskImageFormatIds.AppleIIProDos800));

        Assert.Empty(volume.Warnings);
        Assert.Equal(large.Content, Assert.Single(volume.Entries).Content);
    }

    [Fact]
    public void SosVolumeKeepsDistinctFormatAndFileSystemIdentity()
    {
        var file = CreateFile("SOSFILE", 1_024);
        var plan = new MigrationPlan("source.fs", FileSystemIds.Sos, "SOSVOL", [file]);

        var image = new SosVolumeWriter().Create(plan);
        var volume = new ProDosFileSystemReader().Read(image);

        Assert.Equal(DiskImageFormatIds.AppleIIISos, image.FormatId);
        Assert.Equal(FileSystemIds.Sos, volume.FileSystemId);
        Assert.Equal(file.Content, Assert.Single(volume.Entries).Content);
    }

    private static MigrationEntry CreateFile(string name, int length)
    {
        var content = Enumerable.Range(0, length).Select(index => (byte)(index * 31)).ToArray();
        return new(name, name, FileSystemEntryKind.File, content, DateTimeOffset.Parse("1992-04-10T19:28:00Z"), string.Empty, 0, true, []);
    }
}
