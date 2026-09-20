using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Conversion.Migration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Sos;
using GWGUI.MediaFileSystems.Definitions;
using EngineFileSystemIds = GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds;

namespace GWGUI.Tests.Media;

public sealed class ProDosMigrationImageTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIProDos140)]
    [InlineData(DiskImageFormatIds.AppleIIProDos800)]
    public void MigratedFileCanBeReadFromProDosImage(string formatId)
    {
        var image = new ProDosMigrationImageBuilder().Create(CreatePlan(EngineFileSystemIds.ProDos), formatId);
        AssertFile(image, FileSystemIds.ProDos);
    }

    [Fact]
    public void MigratedFileCanBeReadFromSosImage()
    {
        var image = new SosVolumeWriter().Create(CreatePlan(EngineFileSystemIds.Sos));
        AssertFile(image, FileSystemIds.Sos);
    }

    private static MigrationPlan CreatePlan(string targetFileSystemId) => new(
        EngineFileSystemIds.Fat12,
        targetFileSystemId,
        "VOLUME",
        [new MigrationEntry("/HELLO", "HELLO", FileSystemEntryKind.File,
            new byte[] { 1, 2, 3 }, null, string.Empty, 0, true, [])]);

    private static void AssertFile(GWGUI.MediaEngine.Representations.Sectors.SectorImage image, string expectedFileSystemId)
    {
        var reader = Assert.Single(FileSystemReaderCatalog.CreateDefault(), item => item.Id == FileSystemIds.ProDos);
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal(expectedFileSystemId, volume.FileSystemId);
        var file = Assert.Single(volume.Entries);
        Assert.Equal("HELLO", file.Name);
        var content = Assert.IsAssignableFrom<IReadOnlyList<byte>>(file.Content);
        Assert.Equal(new byte[] { 1, 2, 3 }, content);
    }
}
