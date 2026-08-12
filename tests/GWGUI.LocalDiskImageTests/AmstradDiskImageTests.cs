using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Exploration;
using System.IO;

namespace GWGUI.Tests;

public sealed class AmstradDiskImageTests
{
    [Fact]
    public async Task RealSingleAmstradCpcImageAndFluxRemainEquivalentWhenRequested()
    {
        var dskPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_DSK");
        var scpPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_SCP");
        var sourceSectorsPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_SOURCE_SECTORS");
        var decodedSectorsPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_DECODED_SECTORS");
        if (new[] { dskPath, scpPath, sourceSectorsPath, decodedSectorsPath }.Any(string.IsNullOrWhiteSpace)) return;

        Assert.Equal(
            await File.ReadAllBytesAsync(sourceSectorsPath!),
            await File.ReadAllBytesAsync(decodedSectorsPath!));

        var explorer = DiskImageExplorer.CreateDefault();
        var source = await explorer.ExploreAsync(dskPath!, "amstrad.cpc");
        var flux = await explorer.ExploreAsync(scpPath!, "amstrad.cpc");

        Assert.True(source.FileSystemRecognized, dskPath);
        Assert.True(flux.FileSystemRecognized, scpPath);
        Assert.Equal(source.Volume.Name, flux.Volume.Name);
        Assert.Equal(source.Volume.FileSystemId, flux.Volume.FileSystemId);
        Assert.Equal(source.Volume.Capacity, flux.Volume.Capacity);
        Assert.Equal(source.Volume.FreeBytes, flux.Volume.FreeBytes);
        Assert.Equal(Flatten(source.Volume.Entries), Flatten(flux.Volume.Entries));
        Assert.Equal(source.Volume.Warnings, flux.Volume.Warnings);

        var visualization = new SectorImageFluxVisualizer().Create(source.Image);
        Assert.NotEmpty(visualization.Tracks);
        Assert.Contains(visualization.Tracks, track => track.Head == 0);

        var automatic = await explorer.ExploreAsync(scpPath!);
        Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {scpPath}");
        Assert.Equal("amstrad.cpc", automatic.Image.FormatId);
    }

    [Fact]
    public async Task RealSingleAmstradPcwImageAndFluxRemainEquivalentWhenRequested()
    {
        var dskPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_PCW_DSK");
        var scpPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_PCW_SCP");
        var sourceSectorsPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_PCW_SOURCE_SECTORS");
        var decodedSectorsPath = Environment.GetEnvironmentVariable("GWGUI_REAL_AMSTRAD_PCW_DECODED_SECTORS");
        if (new[] { dskPath, scpPath, sourceSectorsPath, decodedSectorsPath }.Any(string.IsNullOrWhiteSpace)) return;

        Assert.Equal(
            await File.ReadAllBytesAsync(sourceSectorsPath!),
            await File.ReadAllBytesAsync(decodedSectorsPath!));

        var explorer = DiskImageExplorer.CreateDefault();
        var source = await explorer.ExploreAsync(dskPath!, "amstrad.pcw");
        var flux = await explorer.ExploreAsync(scpPath!, "amstrad.pcw");

        Assert.True(source.FileSystemRecognized, dskPath);
        Assert.True(flux.FileSystemRecognized, scpPath);
        Assert.Equal(source.Volume.Name, flux.Volume.Name);
        Assert.Equal(source.Volume.FileSystemId, flux.Volume.FileSystemId);
        Assert.Equal(source.Volume.Capacity, flux.Volume.Capacity);
        Assert.Equal(source.Volume.FreeBytes, flux.Volume.FreeBytes);
        Assert.Equal(Flatten(source.Volume.Entries), Flatten(flux.Volume.Entries));
        Assert.Equal(source.Volume.Warnings, flux.Volume.Warnings);

        var visualization = new SectorImageFluxVisualizer().Create(source.Image);
        Assert.NotEmpty(visualization.Tracks);
        Assert.Contains(visualization.Tracks, track => track.Head == 0);
        Assert.Contains(visualization.Tracks, track => track.Head == 1);

        var automatic = await explorer.ExploreAsync(scpPath!);
        Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {scpPath}");
        Assert.Equal("amstrad.pcw", automatic.Image.FormatId);
    }

    [Fact]
    public async Task RealAmstradDskCorpusCanBeOpened()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_AMSTRAD_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var files = new[] { Path.Combine(root, "Amstrad CPC"), Path.Combine(root, "Amstrad PCW") }
            .Where(Directory.Exists).SelectMany(directory => Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories))
            .Where(path => new[] { ".dsk", ".edsk" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "_generated" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file);
            Assert.True(explored.FileSystemRecognized, file);
            Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.AmstradCpm, explored.Volume.FileSystemId);
            Assert.True(explored.Image.AvailableBlocks.Count > 0, file);
        }
    }

    [Fact]
    public async Task RealAmstradScpCorpusCanBeReconstructed()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_AMSTRAD_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var generated = Path.Combine(root, "_generated");
        if (!Directory.Exists(generated)) return;
        var files = new[] { Path.Combine(generated, "Amstrad CPC"), Path.Combine(generated, "Amstrad PCW") }
            .Where(Directory.Exists).SelectMany(directory => Directory.EnumerateFiles(directory, "*.scp", SearchOption.AllDirectories))
            .OrderBy(path => path).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var format = file.Contains("PCW", StringComparison.OrdinalIgnoreCase) ? "amstrad.pcw" : "amstrad.cpc";
            var explored = await explorer.ExploreAsync(file, format);
            Assert.True(explored.FileSystemRecognized, file);
            var automatic = await explorer.ExploreAsync(file);
            Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {file}");
            Assert.StartsWith("amstrad.", automatic.Image.FormatId);
        }
    }

    private static string[] Flatten(IEnumerable<GWGUI.MediaEngine.FileSystems.FileSystemEntry> entries, string prefix = "") => entries
        .SelectMany(entry => new[]
        {
            $"{prefix}/{entry.Name}|{entry.Kind}|{entry.Size}|{entry.Comment}|{entry.RawAttributes}|{entry.MetadataValid}|{Convert.ToBase64String(entry.Content?.ToArray() ?? [])}"
        }.Concat(Flatten(entry.Children, $"{prefix}/{entry.Name}")))
        .ToArray();
}
