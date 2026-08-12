using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Results;
using System.IO;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

public sealed class DecDiskImageTests
{
    [Fact]
    public async Task RealMincRx02ExposesItsRt11Directory()
    {
        var path = Path.Combine(TestRoot(), "DEC MINC - RX02", "BA-J837B-BC_MINC_MA_DEMO_23_V2.0_BIN_RX2.img");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.True(document.FileSystemRecognized);
        Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Rt11, document.Volume.FileSystemId);
        Assert.NotEmpty(document.Volume.Name);
        Assert.NotEmpty(document.Volume.Entries);
        Assert.All(document.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
        Assert.All(document.Volume.Entries, entry => Assert.True(entry.Size > 0));
        Console.WriteLine($"{document.Volume.Name} | {document.Volume.FileSystemId} | {document.Volume.Capacity} bytes | {document.Volume.FreeBytes} free");
        foreach (var entry in document.Volume.Entries)
            Console.WriteLine($"{entry.Name} | {entry.Size} bytes | {entry.Modified:yyyy-MM-dd} | valid={entry.MetadataValid}");
    }

    [Fact]
    public async Task RealMincSystem03Rx02ExposesItsRt11Directory()
    {
        var path = Path.Combine(TestRoot(), "DEC MINC - RX02", "BA_H106D-BC_MINC_MA_SYS_03_V2.0_BIN_RX2.img");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        AssertValid(document);
        Print(document);
    }

    [Fact]
    public async Task RealMincDemo03Rx02ExposesItsRt11Directory()
    {
        var path = Path.Combine(TestRoot(), "DEC MINC - RX02", "BA_H107D-BC_MINC_MA_DEMO_03_V2.0_BIN_RX2.img");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        AssertValid(document);
        Print(document);
    }

    [Fact]
    public async Task RealMincSystem23Rx02ExposesItsRt11Directory()
    {
        var path = Path.Combine(TestRoot(), "DEC MINC - RX02", "BA_J836B-BC_MINC_MA_SYS_23_V2.0_BIN_RX2.img");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        AssertValid(document);
        Print(document);
    }

    private static void AssertValid(ExploredDiskImage document)
    {
        Assert.True(document.FileSystemRecognized);
        Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Rt11, document.Volume.FileSystemId);
        Assert.NotEmpty(document.Volume.Name);
        Assert.NotEmpty(document.Volume.Entries);
        Assert.All(document.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
        Assert.All(document.Volume.Entries, entry => Assert.True(entry.Size > 0));
    }

    private static void Print(ExploredDiskImage document)
    {
        Console.WriteLine($"{document.Volume.Name} | {document.Volume.FileSystemId} | {document.Volume.Capacity} bytes | {document.Volume.FreeBytes} free | {document.Volume.Entries.Count} files");
        foreach (var entry in document.Volume.Entries)
            Console.WriteLine($"{entry.Name} | {entry.Size} bytes | {entry.Modified:yyyy-MM-dd} | valid={entry.MetadataValid}");
    }

    private static string TestRoot() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
}
