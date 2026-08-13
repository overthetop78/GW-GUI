using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Apple.Dos;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Migration;
using System.IO;

namespace GWGUI.Tests;

public sealed class AppleFileSystemMigrationServiceTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos113, ".d13")]
    [InlineData(DiskImageFormatIds.AppleIIAppleDos140, ".do")]
    public async Task AppleDosMigrationWritesReadableRawContainer(string formatId, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
        try
        {
            var content = Enumerable.Range(0, 400).Select(index => (byte)index).ToArray();
            var plan = new MigrationPlan("source.fs", FileSystemIds.AppleDos, "DOS-001", [new MigrationEntry("HELLO", "HELLO", FileSystemEntryKind.File, content, null, string.Empty, 0, true, [])]);

            await MediaEngineFactory.CreateAppleFileSystemMigrationService().WriteAsync(plan, path, formatId);
            var image = await new AppleDiskImageReader().ReadAsync(path);
            var volume = new AppleDosFileSystemReader().Read(image);

            Assert.Equal(content, Assert.Single(volume.Entries).Content!.Take(content.Length));
            Assert.Empty(volume.Warnings);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AppleIIProDos140, ".po", FileSystemIds.ProDos)]
    [InlineData(DiskImageFormatIds.AppleIIProDos800, ".2mg", FileSystemIds.ProDos)]
    [InlineData(DiskImageFormatIds.AppleIIISos, ".2mg", FileSystemIds.Sos)]
    public async Task ProDosAndSosMigrationsWriteReadableContainers(string formatId, string extension, string expectedFileSystemId)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
        try
        {
            var content = Enumerable.Range(0, 2_000).Select(index => (byte)(index * 7)).ToArray();
            var plan = new MigrationPlan("source.fs", expectedFileSystemId, "TARGET", [new MigrationEntry("DATA", "DATA", FileSystemEntryKind.File, content, null, string.Empty, 0, true, [])]);

            await MediaEngineFactory.CreateAppleFileSystemMigrationService().WriteAsync(plan, path, formatId);
            var image = await new AppleDiskImageReader().ReadAsync(path);
            var volume = new ProDosFileSystemReader().Read(image);

            Assert.Equal(expectedFileSystemId, volume.FileSystemId);
            Assert.Equal(content, Assert.Single(volume.Entries).Content);
            Assert.Empty(volume.Warnings);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void AppleDosMigrationReportsUnsupportedHierarchyAndMetadataBeforeWriting()
    {
        var child = new MigrationEntry("DIR/FILE", "FILE", FileSystemEntryKind.File, [1], DateTimeOffset.UtcNow, "comment", 1, true, []);
        var plan = new MigrationPlan("source.fs", FileSystemIds.AppleDos, "INVALID", [new MigrationEntry("DIR", "DIR", FileSystemEntryKind.Directory, null, null, string.Empty, 0, true, [child])]);

        var report = MigrationValidator.Validate(plan, FileSystemMigrationCapabilityCatalog.ForAppleDos(DiskImageFormatIds.AppleIIAppleDos140));

        Assert.False(report.CanExecute);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.UnsupportedEntryKind);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.InvalidName && loss.Path == "/");
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.ModifiedDate);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.Comment);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.Attributes);
    }
}
