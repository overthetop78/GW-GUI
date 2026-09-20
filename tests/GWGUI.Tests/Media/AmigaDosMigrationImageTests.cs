using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Conversion.Migration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaFileSystems.FileSystems.Amiga;
using EngineFileSystemIds = GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds;
using MediaFileSystemIds = GWGUI.MediaFileSystems.Definitions.FileSystemIds;

namespace GWGUI.Tests.Media;

public sealed class AmigaDosMigrationImageTests
{
    [Fact]
    public void InjectedFileCanBeReadFromTheCreatedAdfImage()
    {
        var plan = new MigrationPlan(
            EngineFileSystemIds.Fat12,
            EngineFileSystemIds.AmigaDosFfs,
            "TEST",
            [new MigrationEntry("/HELLO", "HELLO", FileSystemEntryKind.File,
                new byte[] { 1, 2, 3 }, null, string.Empty, 0, true, [])]);

        var image = new AmigaDosMigrationImageBuilder().Create(
            plan, GWGUI.MediaEngine.FileSystems.Amiga.AmigaDosVariant.Ffs, DiskImageFormatIds.AmigaDos);
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
