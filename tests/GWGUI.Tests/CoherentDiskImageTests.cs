using System.IO;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

public sealed class CoherentDiskImageTests
{
    [Fact]
    public async Task Commodore900CoherentVolumeExposesRealDirectoryAndFiles()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test",
            "COHERENT - ordinateur à identifier", "COHERENT - Volume 1 - High Resolution.bin"));
        if (!File.Exists(path)) return;
        var image = await new CoherentImageReader().ReadAsync(path);
        var volume = new FileSystemRegistry().Read(image);

        Assert.Equal("commodore900.coherent", image.FormatId);
        Assert.Equal("COHERENT (Commodore 900)", volume.FileSystem);
        Assert.NotEmpty(volume.Entries);
        Assert.Contains(volume.Entries, entry => entry.Name == "coherent");
        Assert.All(volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
    }
}
