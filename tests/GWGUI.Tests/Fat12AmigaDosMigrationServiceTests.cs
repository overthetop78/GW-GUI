using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Adf;
using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Amiga;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Migration;
using System.IO;

namespace GWGUI.Tests;

public sealed class Fat12AmigaDosMigrationServiceTests
{
    [Fact]
    public async Task MigratesFat12FilesIntoANewAmigaDosVolume()
    {
        var content = Enumerable.Range(0, 2_000).Select(index => (byte)(index * 7)).ToArray();
        var entry = new MigrationEntry("README.TXT", "README.TXT", FileSystemEntryKind.File, content, DateTimeOffset.Parse("1992-04-10T19:28:00+00:00"), string.Empty, 0, true, []);
        var plan = new MigrationPlan(FileSystemIds.Fat12, FileSystemIds.AmigaDosFfs, "MIGRATED", [entry]);
        var path = TemporaryPath(".adf");
        try
        {
            await MediaEngineFactory.CreateFat12AmigaDosMigrationService().WriteAsync(plan, path, DiskImageFormatIds.AmigaDos);
            var image = await new AdfReader().ReadAsync(path);
            var volume = new AmigaDosFileSystemReader().Read(image);

            Assert.Equal("MIGRATED", volume.Name);
            Assert.Equal(content, Assert.Single(volume.Entries).Content);
            Assert.Empty(volume.Warnings);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task MigratesAmigaDosFilesIntoANewFat12Volume()
    {
        var content = Enumerable.Range(0, 2_000).Select(index => (byte)(index * 11)).ToArray();
        var entry = new MigrationEntry("DATA.BIN", "DATA.BIN", FileSystemEntryKind.File, content, DateTimeOffset.Parse("1992-04-10T19:28:00+00:00"), string.Empty, 0, true, []);
        var plan = new MigrationPlan(FileSystemIds.AmigaDosFfs, FileSystemIds.Fat12, "MIGRATED", [entry]);
        var path = TemporaryPath(".st");
        try
        {
            await MediaEngineFactory.CreateFat12AmigaDosMigrationService().WriteAsync(plan, path, DiskImageFormatIds.AtariSt720);
            var image = await new AtariStReader().ReadAsync(path);
            var volume = new Fat12FileSystemReader().Read(image);

            Assert.Equal("MIGRATED", volume.Name);
            Assert.Equal(content, Assert.Single(volume.Entries).Content);
            Assert.Empty(volume.Warnings);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task RefusesSilentAttributeLossBeforeCreatingTheDestination()
    {
        var entry = new MigrationEntry("DATA.BIN", "DATA.BIN", FileSystemEntryKind.File, [1], null, string.Empty, 1, true, []);
        var plan = new MigrationPlan(FileSystemIds.AmigaDosFfs, FileSystemIds.Fat12, "MIGRATED", [entry]);
        var path = TemporaryPath(".st");
        try
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => MediaEngineFactory.CreateFat12AmigaDosMigrationService().WriteAsync(plan, path, DiskImageFormatIds.AtariSt720));
            Assert.False(File.Exists(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static string TemporaryPath(string extension) => Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
}
