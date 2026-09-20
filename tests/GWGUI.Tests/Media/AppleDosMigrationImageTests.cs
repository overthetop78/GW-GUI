using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Conversion.Migration;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaFileSystems.FileSystems.Apple.Dos;
using EngineFileSystemIds = GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds;
using MediaFileSystemIds = GWGUI.MediaFileSystems.Definitions.FileSystemIds;

namespace GWGUI.Tests.Media;

public sealed class AppleDosMigrationImageTests
{
    [Fact]
    public void InjectedFileCanBeReadFromTheCreatedDosImage()
    {
        var plan = new MigrationPlan(
            EngineFileSystemIds.Fat12,
            EngineFileSystemIds.AppleDos,
            "DOS-001",
            [new MigrationEntry("/HELLO", "HELLO", FileSystemEntryKind.File,
                new byte[] { 1, 2, 3 }, null, string.Empty, 0, true, [])]);

        var image = new AppleDosMigrationImageBuilder().Create(plan, DiskImageFormatIds.AppleIIAppleDos140);
        var reader = new AppleDosFileSystemReader();

        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Equal("DOS-001", volume.Name);
        Assert.Equal(MediaFileSystemIds.AppleDos, volume.FileSystemId);
        var file = Assert.Single(volume.Entries);
        Assert.Equal("HELLO", file.Name);
        Assert.Equal(new byte[] { 1, 2, 3 }, file.Content);
    }
}
