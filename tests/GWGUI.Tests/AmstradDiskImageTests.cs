using System.IO;
using GWGUI.Scp.Images;

namespace GWGUI.Tests;

public sealed class AmstradDiskImageTests
{
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
            Assert.Contains("Amstrad", explored.Volume.FileSystem);
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
}
