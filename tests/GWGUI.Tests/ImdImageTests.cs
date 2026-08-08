using System.IO;
using GWGUI.Scp.Images;

namespace GWGUI.Tests;

public sealed class ImdImageTests
{
    [Fact]
    public async Task FirstRealEpsonImageCanBeExplored()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "_generated", "Epson QX-10", "Valdocs 2.00 disk01-396.imd");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Console.WriteLine($"{document.Image.FormatId} | {document.Image.Capacity} bytes | {document.Image.BlockSize}-byte main blocks | {document.Image.Cylinders} cylinders | {document.Image.Heads} heads | {document.Image.AvailableBlocks.Count}/{document.Image.BlockCount} sectors | {document.Volume.FileSystem} | {document.Volume.Entries.Count} entries | {document.Volume.Warnings.Count} warning(s)");
        Console.WriteLine(string.Join(", ", document.Image.AvailableBlocks.GroupBy(block => block.Data.Count).Select(group => $"{group.Key}x{group.Count()}")));
        using (var flattened = new MemoryStream())
        {
            foreach (var block in document.Image.AvailableBlocks.OrderBy(block => block.LogicalBlock)) flattened.Write(block.Data.ToArray());
            var bytes = flattened.ToArray();
            var candidates = new List<(int Offset, int Score)>();
            for (var offset = 0; offset + 2048 <= bytes.Length; offset += 256)
            {
                var score = 0;
                for (var entryOffset = offset; entryOffset < offset + 2048; entryOffset += 32)
                {
                    var entry = bytes.AsSpan(entryOffset, 32);
                    if (entry[0] <= 31 && entry[1..12].ToArray().All(value => (value & 0x7f) == 0x20 || (value & 0x7f) is >= 0x21 and <= 0x7e)) score++;
                }
                if (score > 0) candidates.Add((offset, score));
            }
            Console.WriteLine(string.Join(", ", candidates.OrderByDescending(item => item.Score).Take(10)));
        }
        Assert.NotEqual("unknown", document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
    }

    [Fact]
    public async Task SecondRealEpsonImageCanBeExplored()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "_generated", "Epson QX-10", "Valdocs 2.00 disk01.imd");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Console.WriteLine($"{document.Image.FormatId} | {document.Image.Capacity} bytes | {document.Image.BlockSize}-byte main blocks | {document.Image.Cylinders} cylinders | {document.Image.Heads} heads | {document.Image.AvailableBlocks.Count}/{document.Image.BlockCount} sectors | {document.Volume.FileSystem} | {document.Volume.Entries.Count} entries | {document.Volume.Warnings.Count} warning(s)");
        Console.WriteLine(string.Join(", ", document.Image.AvailableBlocks.GroupBy(block => block.Data.Count).Select(group => $"{group.Key}x{group.Count()}")));
        ReportDirectoryCandidates(document.Image);
        Assert.NotEqual("unknown", document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
    }

    private static void ReportDirectoryCandidates(GWGUI.Scp.SectorImages.SectorImage image)
    {
        using var flattened = new MemoryStream();
        foreach (var block in image.AvailableBlocks.OrderBy(block => block.LogicalBlock)) flattened.Write(block.Data.ToArray());
        var bytes = flattened.ToArray();
        var candidates = new List<(int Offset, int Score)>();
        for (var offset = 0; offset + 2048 <= bytes.Length; offset += 256)
        {
            var score = 0;
            for (var entryOffset = offset; entryOffset < offset + 2048; entryOffset += 32)
            {
                var entry = bytes.AsSpan(entryOffset, 32);
                if (entry[0] <= 31 && entry[1..12].ToArray().All(value => (value & 0x7f) == 0x20 || (value & 0x7f) is >= 0x21 and <= 0x7e)) score++;
            }
            if (score > 0) candidates.Add((offset, score));
        }
        Console.WriteLine(string.Join(", ", candidates.OrderByDescending(item => item.Score).Take(10)));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
