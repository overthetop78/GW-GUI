using System.IO;
using GWGUI.Scp.Images;
using Xunit.Abstractions;

namespace GWGUI.Tests;

public sealed class EpsonQx10DiskImageTests(ITestOutputHelper output)
{
    [Fact]
    public async Task RealValdocsDisk01ExposesItsTpmDirectory()
    {
        await Verify("Valdocs 2.00 for Epson QX-10 disk01.scp", automatic: true);
    }

    [Fact] public Task RealValdocsDisk02ExposesItsTpmDirectory() => Verify("Valdocs 2.00 for Epson QX-10 disk02.scp");
    [Fact] public Task RealValdocsDisk03ExposesItsTpmDirectory() => Verify("Valdocs 2.00 for Epson QX-10 disk03.scp");
    [Fact] public Task RealValdocsDisk04ExposesItsTpmDirectory() => Verify("Valdocs 2.00 for Epson QX-10 disk04.scp");
    [Fact] public Task RealValdocsDisk05ExposesItsTpmDirectory() => Verify("Valdocs 2.00 for Epson QX-10 disk05.scp");

    private async Task Verify(string fileName, bool automatic = false)
    {
        var path = TestImage("Epson QX-10", fileName);
        if (!File.Exists(path)) return;
        var disk = await DiskImageExplorer.CreateDefault().ExploreAsync(path, automatic ? null : "epson.qx10.396");
        output.WriteLine($"Format={disk.Image.FormatId}; FS={disk.Volume.FileSystem}; Volume='{disk.Volume.Name}'; Capacity={disk.Volume.Capacity}; Free={disk.Volume.FreeBytes}");
        foreach (var entry in disk.Volume.Entries)
            output.WriteLine($"{entry.Name}\t{entry.Size}\t{entry.Modified:O}\tvalid={entry.MetadataValid}");
        Assert.True(disk.FileSystemRecognized);
        Assert.Equal("epson.qx10.396", disk.Image.FormatId);
        Assert.Contains("CP/M", disk.Volume.FileSystem, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(disk.Volume.Entries);
        Assert.All(disk.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
    }

    private static string TestImage(params string[] parts)
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
        return parts.Aggregate(path, Path.Combine);
    }
}
