using System.IO;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

public sealed class IbmPcDiskImageTests
{
    [Theory]
    [InlineData(160 * 1024, "ibm.160", 40, 1, 8)]
    [InlineData(180 * 1024, "ibm.180", 40, 1, 9)]
    [InlineData(320 * 1024, "ibm.320", 40, 2, 8)]
    [InlineData(360 * 1024, "ibm.360", 40, 2, 9)]
    [InlineData(720 * 1024, "ibm.720", 80, 2, 9)]
    [InlineData(1200 * 1024, "ibm.1200", 80, 2, 15)]
    [InlineData(1440 * 1024, "ibm.1440", 80, 2, 18)]
    [InlineData(1680 * 1024, "ibm.1680", 80, 2, 21)]
    [InlineData(2880 * 1024, "ibm.2880", 80, 2, 36)]
    public async Task RawReaderSupportsStandardGeometries(int length, string format, int cylinders, int heads, int sectors)
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".ima");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[length]);
            var image = await new IbmPcImageReader().ReadAsync(path);
            Assert.Equal(format, image.FormatId);
            Assert.Equal(cylinders, image.Cylinders);
            Assert.Equal(heads, image.Heads);
            Assert.Equal(sectors, image.SectorsPerTrack);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RawReaderRejectsANonAlignedStandardCapacity()
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".ima");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[720 * 1024 + 1]);
            await Assert.ThrowsAsync<InvalidDataException>(() => new IbmPcImageReader().ReadAsync(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RealIbmPcRawCorpusCanBeOpened()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_IBM_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var directory = Path.Combine(root, "IBM PC");
        if (!Directory.Exists(directory)) return;
        var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(path => new[] { ".img", ".ima" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file);
            Assert.True(explored.FileSystemRecognized,
                $"{file}; format={explored.Image.FormatId}; geometry={explored.Image.Cylinders}x{explored.Image.Heads}x{explored.Image.SectorsPerTrack}; blocks={explored.Image.AvailableBlocks.Count}; missing={explored.Image.MissingBlocks.Count}");
            Assert.Equal("IBM PC FAT12", explored.Volume.FileSystem);
            Assert.StartsWith("ibm.", explored.Image.FormatId);
            var automatic = await explorer.ExploreAsync(file);
            Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {file}");
            Assert.True(automatic.Volume.FileSystem == "IBM PC FAT12", $"{file}; automatic file system={automatic.Volume.FileSystem}; format={automatic.Image.FormatId}");
            Assert.StartsWith("ibm.", automatic.Image.FormatId);
        }
    }

    [Fact]
    public async Task RealIbmPcScpCorpusCanBeReconstructed()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_IBM_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var directory = Path.Combine(root, "IBM PC");
        if (!Directory.Exists(directory)) return;
        var files = Directory.EnumerateFiles(directory, "*.scp", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file, "ibm.scan");
            Assert.True(explored.FileSystemRecognized,
                $"{file}; format={explored.Image.FormatId}; geometry={explored.Image.Cylinders}x{explored.Image.Heads}x{explored.Image.SectorsPerTrack}; blocks={explored.Image.AvailableBlocks.Count}; missing={explored.Image.MissingBlocks.Count}");
            Assert.Equal("IBM PC FAT12", explored.Volume.FileSystem);
            Assert.StartsWith("ibm.", explored.Image.FormatId);
            var automatic = await explorer.ExploreAsync(file);
            Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {file}");
            Assert.True(automatic.Volume.FileSystem == "IBM PC FAT12", $"{file}; automatic file system={automatic.Volume.FileSystem}; format={automatic.Image.FormatId}");
            Assert.StartsWith("ibm.", automatic.Image.FormatId);
        }
    }
}
