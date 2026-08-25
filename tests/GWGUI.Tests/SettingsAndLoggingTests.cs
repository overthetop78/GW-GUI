using GWGUI.App.Functions.Services.Conversion;
using GWGUI.App.Services.Storage;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Parsing;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Emulation;
using GWGUI.Domain.Settings.Logging;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using System.IO;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Hardware;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class SettingsAndLoggingTests : CoreTestBase
{
    [Fact]
    public void PortableMarkerMovesSettingsNextToTheApplication()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-portable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            Assert.Equal(Path.Combine("roaming", "GW GUI"), StoragePaths.ResolveDataDirectory(directory, "roaming"));
            File.WriteAllText(Path.Combine(directory, "portable.flag"), "");
            Assert.Equal(Path.Combine(directory, "Data"), StoragePaths.ResolveDataDirectory(directory, "roaming"));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void HostToolsDirectoryUsesTheRoamingDataDirectory()
    {
        Assert.Equal(Path.Combine(StoragePaths.DataDirectory, "Greaseweazle"),
            StoragePaths.HostToolsDirectory);
    }

    [Fact]
    public async Task VersionOneSettingsMigrateFormatIdentifiersAndCollections()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            await File.WriteAllTextAsync(path, """{"SchemaVersion":1,"Read":{"FormatId":"amiga.amigadoshd"},"Conversion":{"SelectedFormats":["amiga.amigadoshd"],"ExplicitExtensions":{"amiga.amigadoshd":[".adf"]}},"Profiles":[{"Operation":"Convert","Name":"HD","EnabledOptions":["format:amiga.amigadoshd"],"Values":{"extensions:amiga.amigadoshd":".adf"}}]}""");
            var settings = await new JsonSettingsStore(path).LoadAsync();

            Assert.Equal(SettingsMigrator.CurrentVersion, settings.SchemaVersion);
            Assert.Equal("amiga.amigados_hd", settings.Read.FormatId);
            Assert.Contains("amiga.amigados_hd", settings.Conversion.SelectedFormats);
            Assert.Contains("amiga.amigados_hd", settings.Conversion.ExplicitExtensions.Keys);
            Assert.Contains("format:amiga.amigados_hd", settings.Profiles[0].EnabledOptions);
            Assert.Contains("extensions:amiga.amigados_hd", settings.Profiles[0].Values.Keys);
            Assert.NotNull(settings.Write);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task InvalidSettingsRecoverFromLastBackupAndArePreserved()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new JsonSettingsStore(path);
            await store.SaveAsync(new AppSettings { Language = "fr" });
            await store.SaveAsync(new AppSettings { Language = "en" });
            await File.WriteAllTextAsync(path, "{ invalid json");

            var recovered = await store.LoadAsync();

            Assert.Equal("fr", recovered.Language);
            Assert.Contains(Directory.GetFiles(directory), file => file.Contains(".invalid-", StringComparison.Ordinal));
            Assert.Contains("\"Language\": \"fr\"", await File.ReadAllTextAsync(path));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task LastDiskImageFolderIsPersistedIndependentlyFromReadDestination()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-last-image-folder-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new JsonSettingsStore(path);
            await store.SaveAsync(new AppSettings
            {
                DefaultImagesFolder = @"F:\Read destination",
                LastDiskImageFolder = @"F:\Disk images\Atari"
            });

            var restored = await store.LoadAsync();

            Assert.Equal(@"F:\Read destination", restored.DefaultImagesFolder);
            Assert.Equal(@"F:\Disk images\Atari", restored.LastDiskImageFolder);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task EmulationMediaFoldersArePersistedPerMachineAndSupportKind()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-emulation-media-folders-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "settings.json");
        try
        {
            var store = new JsonSettingsStore(path);
            await store.SaveAsync(new AppSettings
            {
                EmulationMediaFolders =
                [
                    new() { ModuleId = "atari", MachineId = "St", Category = EmulationMediaFolderCategory.Floppy, Folder = @"F:\Atari\Disks" },
                    new() { ModuleId = "amiga", MachineId = "A500", Category = EmulationMediaFolderCategory.Floppy, Folder = @"F:\Amiga\Disks" },
                    new() { ModuleId = "amiga", MachineId = "Cd32", Category = EmulationMediaFolderCategory.CompactDisc, Folder = @"F:\Amiga\CD" }
                ]
            });

            var restored = await store.LoadAsync();

            Assert.Contains(restored.EmulationMediaFolders, item => item.ModuleId == "atari"
                && item.MachineId == "St" && item.Category == EmulationMediaFolderCategory.Floppy && item.Folder == @"F:\Atari\Disks");
            Assert.Contains(restored.EmulationMediaFolders, item => item.ModuleId == "amiga"
                && item.MachineId == "A500" && item.Category == EmulationMediaFolderCategory.Floppy && item.Folder == @"F:\Amiga\Disks");
            Assert.Contains(restored.EmulationMediaFolders, item => item.ModuleId == "amiga"
                && item.MachineId == "Cd32" && item.Category == EmulationMediaFolderCategory.CompactDisc && item.Folder == @"F:\Amiga\CD");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task OperationLogWriterRotatesAndKeepsCommandAndOutput()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-log-" + Guid.NewGuid().ToString("N"));
        try
        {
            var writer = new RotatingOperationLogWriter(directory, maximumBytes: 220, maximumFiles: 3);
            var command = new GwCommand("gw.exe", "read", ["disk.scp"]);
            for (var index = 0; index < 5; index++)
            {
                var line = new GwOutputLine(DateTimeOffset.UtcNow, GwOutputStream.Standard, $"T{index}.0: " + new string('x', 90));
                await writer.WriteAsync(command, new GwExecutionResult(0, false, TimeSpan.FromSeconds(1), [line]));
            }

            var files = Directory.GetFiles(directory, "operations*.log");
            Assert.Equal(3, files.Length);
            var current = await File.ReadAllTextAsync(Path.Combine(directory, "operations.log"));
            Assert.Contains("gw.exe read disk.scp", current);
            Assert.Contains("T4.0", current);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task ConsoleLogsUseOneFilePerActionAndTrimOldLines()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-console-log-" + Guid.NewGuid().ToString("N"));
        var settings = new OperationLogSettings { Enabled = true, MaximumKilobytes = 1, KeepArchives = false };
        try
        {
            var logger = new ConsoleLogSession(directory, () => settings);
            await logger.BeginAsync("read", "gw.exe read disk.scp");
            for (var index = 0; index < 40; index++) await logger.AppendAsync($"T{index}.0: {new string('x', 80)}");

            var path = Path.Combine(directory, "read.log");
            Assert.True(File.Exists(path));
            Assert.True(new FileInfo(path).Length <= 1024);
            var text = await File.ReadAllTextAsync(path);
            Assert.Contains("T39.0", text);
            Assert.DoesNotContain("T0.0", text);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task ConsoleLogsCanArchiveWithTimestampAndBeDisabled()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-console-archive-" + Guid.NewGuid().ToString("N"));
        var settings = new OperationLogSettings { Enabled = true, MaximumKilobytes = 1, KeepArchives = true };
        try
        {
            var logger = new ConsoleLogSession(directory, () => settings);
            await logger.BeginAsync("write", "gw.exe write disk.adf");
            for (var index = 0; index < 20; index++) await logger.AppendAsync(new string('x', 100));
            Assert.NotEmpty(Directory.GetFiles(directory, "write-*.log"));
            Assert.True(File.Exists(Path.Combine(directory, "write.log")));

            settings.Enabled = false;
            await logger.BeginAsync("convert", "gw.exe convert source.scp target.ima");
            await logger.AppendAsync("hidden");
            Assert.False(File.Exists(Path.Combine(directory, "convert.log")));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task ConsoleLogSettingsAreIndependentForEachAction()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-action-logs-" + Guid.NewGuid().ToString("N"));
        var settings = new OperationLogSettings();
        settings.GetOrCreate("read").MaximumKilobytes = 2;
        settings.GetOrCreate("write").Enabled = false;
        try
        {
            var read = new ConsoleLogSession(directory, () => settings);
            await read.BeginAsync("read", "gw.exe read disk.scp");
            await read.AppendAsync("read output");
            var write = new ConsoleLogSession(directory, () => settings);
            await write.BeginAsync("write", "gw.exe write disk.scp");
            await write.AppendAsync("write output");

            Assert.True(File.Exists(Path.Combine(directory, "read.log")));
            Assert.False(File.Exists(Path.Combine(directory, "write.log")));
            Assert.Equal(2, settings.ForAction("read").MaximumKilobytes);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task CancelledReadOutputCleanerDeletesOnlyTheRequestedFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-cancelled-read-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var incomplete = Path.Combine(directory, "incomplete.scp");
        var other = Path.Combine(directory, "keep.scp");
        try
        {
            await File.WriteAllTextAsync(incomplete, "partial");
            await File.WriteAllTextAsync(other, "keep");

            Assert.Null(CancelledOutputCleaner.TryDelete(incomplete));
            Assert.False(File.Exists(incomplete));
            Assert.True(File.Exists(other));
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }

    [Fact]
    public void GwHelpCapabilitiesAreParsedBySection()
    {
        const string help = """
            options:
              --format FORMAT

            FORMAT options:
              acorn.adfs.800  amiga.amigados  amiga.amigadoshd
              atarist.720     ibm.720         ibm.scan

            Supported file suffixes:
              .adf  .hfe  .ima  .img  .scp
            """;

        var capabilities = GwFormatCapabilitiesParser.ParseReadHelp(help);

        Assert.Contains("amiga.amigados", capabilities.FormatIds);
        Assert.Contains("ibm.scan", capabilities.FormatIds);
        Assert.DoesNotContain("--format", capabilities.FormatIds);
        Assert.Equal(6, capabilities.FormatIds.Count);
        Assert.Contains(".scp", capabilities.ImageExtensions);
        Assert.Equal(5, capabilities.ImageExtensions.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unrelated output")]
    public void MissingHelpSectionsReturnUnknownCapabilities(string? help)
    {
        Assert.False(GwFormatCapabilitiesParser.ParseReadHelp(help).IsKnown);
    }

    [Fact]
    public void RuntimeCapabilitiesFilterCuratedFormatsAndExtensions()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.720"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".scp", ".img"], StringComparer.OrdinalIgnoreCase));

        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);

        Assert.Contains(catalog.Formats, format => format.Id == "raw.scp");
        var ibm = Assert.Single(catalog.Formats, format => format.Id == "ibm.720");
        Assert.Equal(".img", Assert.Single(ibm.Extensions).Extension);
        Assert.True(ibm.Extensions[0].IsDefault);
        Assert.DoesNotContain(catalog.Formats, format => format.Id == "atarist.720");
    }
}
