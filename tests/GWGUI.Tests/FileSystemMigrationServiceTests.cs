using System.IO;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Migration;

namespace GWGUI.Tests;

public sealed class FileSystemMigrationServiceTests
{
    [Fact]
    public async Task MigratesACommodoreVolumeToAnIbmFat12Image()
    {
        var content = Enumerable.Range(0, 2_000).Select(index => (byte)(index * 13)).ToArray();
        var entry = new FileSystemEntry("DATA.BIN", FileSystemEntryKind.File, content.Length, null, string.Empty, 0, 1, true, [], content);
        var source = new FileSystemVolume("MIGRATED", FileSystemIds.CommodoreDos, 174_848, 0, null, null, [entry], []);
        var path = TemporaryPath(".img");
        try
        {
            var service = MediaEngineFactory.CreateFileSystemMigrationService();
            var report = await service.WriteAsync(source, path, DiskImageFormatIds.Ibm720);
            var image = await new IbmRawImageReader().ReadAsync(path);
            var volume = new Fat12FileSystemReader().Read(image);

            Assert.True(report.CanExecute);
            Assert.Equal("MIGRATED", volume.Name);
            Assert.Equal(content, Assert.Single(volume.Entries).Content);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void CatalogContainsEveryReconstructibleFileSystemFamily()
    {
        var fileSystems = FileSystemMigrationTargetCatalog.All.Select(target => target.FileSystemId).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains(FileSystemIds.AmigaDosFfs, fileSystems);
        Assert.Contains(FileSystemIds.Fat12, fileSystems);
        Assert.Contains(FileSystemIds.AppleDos, fileSystems);
        Assert.Contains(FileSystemIds.ProDos, fileSystems);
        Assert.Contains(FileSystemIds.Sos, fileSystems);
        Assert.Contains(FileSystemIds.CommodoreDos, fileSystems);
    }

    private static string TemporaryPath(string extension) => Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
}
