using GWGUI.MediaEngine.Contracts.Migration;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Creation;
using GWGUI.MediaFileSystems.FileSystems.Amiga;
using MediaFileSystemIds = GWGUI.MediaFileSystems.Definitions.FileSystemIds;

namespace GWGUI.Tests.Media;

public sealed class AmigaDosMigrationImageTests
{
    [Fact]
    public void InjectedFileCanBeReadFromTheCreatedAdfImage()
    {
        var source = new FileSystemVolume("TEST", MediaFileSystemIds.Fat12, 0, 0, null, null,
            [new FileSystemEntry("HELLO", FileSystemEntryKind.File, 3, null, string.Empty,
                0, 0, true, [], new byte[] { 1, 2, 3 })], []);
        var result = new FileSystemMigrationService().CreateImage(source, DiskImageFormatIds.AmigaDos);
        Assert.True(result.Report.CanExecute);
        var image = result.Image;
        var reader = new AmigaDosFileSystemReader();

        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal("TEST", volume.Name);
        Assert.Equal(MediaFileSystemIds.AmigaDosFfs, volume.FileSystemId);
        var file = Assert.Single(volume.Entries);
        Assert.Equal("HELLO", file.Name);
        Assert.Equal(new byte[] { 1, 2, 3 }, file.Content);
    }
}
