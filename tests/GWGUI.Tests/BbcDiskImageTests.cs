using System.IO;
using GWGUI.Scp.Images;

namespace GWGUI.Tests;

public sealed class BbcDiskImageTests
{
    [Fact]
    public async Task RealBbcSsdExposesItsDfsCatalogue()
    {
        var path = Path.Combine(TestRoot(), "BBC Micro", "seeds-of-evil-bbc.ssd");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.True(document.FileSystemRecognized);
        Assert.Equal("Acorn DFS", document.Volume.FileSystem);
        Assert.Equal("The Seeds of", document.Volume.Name);
        Assert.Equal(204_800, document.Volume.Capacity);
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "BUILD" && entry.Size > 0);
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "!BOOT" && entry.Size > 0);
        Assert.All(document.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
    }

    private static string TestRoot() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
}
