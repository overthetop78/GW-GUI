using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Migration;
using System.Diagnostics;
using System.IO;

namespace GWGUI.Tests;

public sealed class CommodoreDosMigrationServiceTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.Commodore1541, DiskImageFileExtensions.D64)]
    [InlineData(DiskImageFormatIds.Commodore1571, DiskImageFileExtensions.D71)]
    [InlineData(DiskImageFormatIds.Commodore1581, DiskImageFileExtensions.D81)]
    public async Task MigrationWritesReadableCommodoreContainers(string formatId, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
        try
        {
            var content = Enumerable.Range(0, 2_000).Select(index => (byte)(index * 11)).ToArray();
            var plan = new MigrationPlan("source.fs", FileSystemIds.CommodoreDos, "TARGET", [new MigrationEntry("HELLO", "HELLO", FileSystemEntryKind.File, content, null, string.Empty, 0, true, [])]);

            await MediaEngineFactory.CreateCommodoreDosMigrationService().WriteAsync(plan, path, formatId, policy: new(CommodoreDosFileType.Prg));
            var image = extension switch
            {
                DiskImageFileExtensions.D64 => await new D64Reader().ReadAsync(path),
                DiskImageFileExtensions.D71 => await new D71Reader().ReadAsync(path),
                _ => await new D81Reader().ReadAsync(path)
            };
            var volume = new CommodoreDosFileSystemReader().Read(image);

            Assert.Empty(volume.Warnings);
            Assert.Equal(content, Assert.Single(volume.Entries).Content);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void MigrationReportsFlatCatalogAndMetadataLosses()
    {
        var child = new MigrationEntry("DIR/FILE", "FILE", FileSystemEntryKind.File, [1], DateTimeOffset.UtcNow, "comment", 1, true, []);
        var plan = new MigrationPlan("source.fs", FileSystemIds.CommodoreDos, "TARGET", [new MigrationEntry("DIR", "DIR", FileSystemEntryKind.Directory, null, null, string.Empty, 0, true, [child])]);

        var report = MigrationValidator.Validate(plan, FileSystemMigrationCapabilityCatalog.ForCommodoreDos(DiskImageFormatIds.Commodore1541));

        Assert.False(report.CanExecute);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.UnsupportedEntryKind);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.ModifiedDate);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.Comment);
        Assert.Contains(report.Losses, loss => loss.Kind == MigrationLossKind.Attributes);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.Commodore1541, DiskImageFileExtensions.D64)]
    [InlineData(DiskImageFormatIds.Commodore1571, DiskImageFileExtensions.D71)]
    [InlineData(DiskImageFormatIds.Commodore1581, DiskImageFileExtensions.D81)]
    public async Task ProducedContainersAreAcceptedByViceC1541WhenAvailable(string formatId, string extension)
    {
        var c1541 = Environment.GetEnvironmentVariable("GWGUI_C1541");
        if (string.IsNullOrWhiteSpace(c1541) || !File.Exists(c1541)) return;
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
        var extractedPath = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.bin");
        try
        {
            var content = Enumerable.Range(0, 600).Select(index => (byte)(index * 13)).ToArray();
            var plan = new MigrationPlan("source.fs", FileSystemIds.CommodoreDos, "VICE TEST", [new MigrationEntry("HELLO", "HELLO", FileSystemEntryKind.File, content, null, string.Empty, 0, true, [])]);
            await MediaEngineFactory.CreateCommodoreDosMigrationService().WriteAsync(plan, path, formatId);

            var start = new ProcessStartInfo(c1541, $"-attach \"{path}\" -bam -read hello \"{Path.GetFileName(extractedPath)}\" -list") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, WorkingDirectory = Path.GetTempPath() };
            using var process = Process.Start(start)!;
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            Assert.True(process.ExitCode == 0, error);
            Assert.Contains("HELLO", output, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(content, await File.ReadAllBytesAsync(extractedPath));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(extractedPath)) File.Delete(extractedPath);
        }
    }
}
