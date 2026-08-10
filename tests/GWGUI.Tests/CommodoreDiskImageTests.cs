using System.IO;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

public sealed class CommodoreDiskImageTests
{
    [Theory]
    [InlineData(174848, 35, 683)]
    [InlineData(196608, 40, 768)]
    public async Task D64ReaderSupportsStandardGeometries(int length, int tracks, int blocks)
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d64");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[length]);
            var image = await new CommodoreD64ImageReader().ReadAsync(path);
            Assert.Equal("commodore.1541", image.FormatId);
            Assert.Equal(tracks, image.Cylinders);
            Assert.Equal(blocks, image.BlockCount);
            Assert.Equal(length, image.Capacity);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task D81ReaderBuildsCbmLogicalSectors()
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d81");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[CommodoreD81ImageReader.ImageBytes]);
            var image = await new CommodoreD81ImageReader().ReadAsync(path);
            Assert.Equal("commodore.1581", image.FormatId);
            Assert.Equal(3_200, image.BlockCount);
            Assert.Equal(256, image.BlockSize);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(349696, 35, 1366)]
    [InlineData(393216, 40, 1536)]
    public async Task D71ReaderSupportsBothSidesAndExtendedGeometries(int length, int tracks, int blocks)
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d71");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[length]);
            var image = await new CommodoreD71ImageReader().ReadAsync(path);
            Assert.Equal("commodore.1571", image.FormatId);
            Assert.Equal(tracks, image.Cylinders);
            Assert.Equal(2, image.Heads);
            Assert.Equal(blocks, image.BlockCount);
            Assert.Equal(length, image.Capacity);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RealCommodoreCorpusCanBeOpened()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_COMMODORE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(path => new[] { ".d64", ".d71", ".d81" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file);
            Assert.True(explored.FileSystemRecognized, file);
            Assert.False(string.IsNullOrWhiteSpace(explored.Volume.FileSystem), file);
            Assert.True(explored.Volume.Entries.Count > 0, file);
            var expected = Path.GetFileName(file).Contains("cpm", StringComparison.OrdinalIgnoreCase) ? "CP/M 3" : "CBM DOS";
            Assert.Equal(expected, explored.Volume.FileSystem);
        }
    }

    [Fact]
    public async Task RealCommodoreScpCorpusCanBeReconstructed()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_COMMODORE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var generatedRoot = Path.Combine(root, "_generated");
        if (!Directory.Exists(generatedRoot)) return;
        var originals = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(path => !path.StartsWith(generatedRoot, StringComparison.OrdinalIgnoreCase))
            .Where(path => new[] { ".d64", ".d71", ".d81" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .GroupBy(path => (Directory: Path.GetRelativePath(root, Path.GetDirectoryName(path)!), Name: Path.GetFileNameWithoutExtension(path)))
            .Select(group => group.OrderByDescending(path => Path.GetExtension(path).Equals(".d81", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(path => Path.GetExtension(path).Equals(".d71", StringComparison.OrdinalIgnoreCase)).First()).ToArray();
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var original in originals)
        {
            var relativeDirectory = Path.GetRelativePath(root, Path.GetDirectoryName(original)!);
            var generated = Path.Combine(generatedRoot, relativeDirectory, Path.GetFileNameWithoutExtension(original) + " [test].scp");
            if (!File.Exists(generated)) continue;
            var formatId = Path.GetExtension(original).ToLowerInvariant() switch { ".d71" => "commodore.1571", ".d81" => "commodore.1581", _ => "commodore.1541" };
            ExploredDiskImage explored;
            try { explored = await explorer.ExploreAsync(generated, formatId); }
            catch (Exception exception) { throw new InvalidDataException(generated, exception); }
            var decodedTracks = string.Join(", ", explored.Image.AvailableBlocks
                .GroupBy(block => block.Address.Cylinder)
                .OrderBy(group => group.Key)
                .Select(group => $"T{group.Key + 1}:{group.Count()}"));
            Assert.True(explored.FileSystemRecognized, $"{generated}; format={explored.Image.FormatId}; blocks={explored.Image.AvailableBlocks.Count}; missing={explored.Image.MissingBlocks.Count}; {decodedTracks}");
            Assert.True(explored.Volume.Entries.Count > 0, generated);
            var automatic = await explorer.ExploreAsync(generated);
            Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {generated}");
            Assert.StartsWith("commodore.", automatic.Image.FormatId);
        }
    }
}
