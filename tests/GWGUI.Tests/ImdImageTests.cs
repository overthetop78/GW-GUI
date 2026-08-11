using System.IO;
using GWGUI.MediaEngine.Containers.ImageDisk;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

public sealed class ImdImageTests
{
    [Fact]
    public async Task UnavailableImdSectorRemainsMissing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-imd-{Guid.NewGuid():N}.imd");
        try
        {
            // One 128-byte sector is declared, but record type 0 means that
            // ImageDisk could not provide its contents.
            await File.WriteAllBytesAsync(path,
            [
                (byte)'I', (byte)'M', (byte)'D', 0x1a,
                0, 0, 0, 1, 0,
                1,
                0
            ]);

            var image = await new ImdReader().ReadAsync(path);

            Assert.Equal(1, image.BlockCount);
            Assert.Equal(128, image.Capacity);
            Assert.Empty(image.AvailableBlocks);
            Assert.Equal([0], image.MissingBlocks);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task PartialEpsonQx10ImagePreservesItsInvalidSectors()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "validated_images", "Epson", "QX-10", "5.25 pouces - QX-10 396 Kio", "Valdocs 2.00 disk01-396.imd");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.Equal("epson.qx10.396", document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
        Assert.Equal(48, document.Image.AvailableBlocks.Count(block => block.IntegrityValid == false));
        Assert.NotEmpty(new SectorImageFluxVisualizer().Create(document.Image).Tracks);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
