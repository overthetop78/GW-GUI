using GWGUI.MediaEngine.Exploration;
using System.IO;
using GWGUI.MediaEngine.Images;
using Xunit.Abstractions;

namespace GWGUI.Tests;

public sealed class MsxDiskImageTests(ITestOutputHelper output)
{
    [Fact]
    public async Task RealSeedsOfEvilDiskExposesItsMsxDosFiles()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "MSX", "seeds-of-evil-msx.dsk"));
        if (!File.Exists(path)) return;
        var disk = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        output.WriteLine($"Format={disk.Image.FormatId}; FS={disk.Volume.FileSystemId}; Volume='{disk.Volume.Name}'; Capacity={disk.Volume.Capacity}; Free={disk.Volume.FreeBytes}");
        foreach (var entry in disk.Volume.Entries)
            output.WriteLine($"{entry.Name}\t{entry.Kind}\t{entry.Size}\t{entry.Modified:O}\tvalid={entry.MetadataValid}");
        Assert.True(disk.FileSystemRecognized);
        Assert.Equal("msx.2dd", disk.Image.FormatId);
        Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Fat12, disk.Volume.FileSystemId);
        Assert.NotEmpty(disk.Volume.Entries);
        Assert.All(disk.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
    }
}
