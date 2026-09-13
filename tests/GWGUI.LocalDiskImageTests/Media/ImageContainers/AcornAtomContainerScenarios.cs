using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems.Acorn.BbcDfs;
using GWGUI.MediaEngine.Formats.Floppy.AcornAtom;

namespace GWGUI.Tests.Media.ImageContainers;

internal static class AcornAtomContainerScenarios
{
    public static async Task ReadAndExplore()
    {
        var data = CreateImage();
        var reader = new AcornAtomDskReader((path, token) =>
        {
            Assert.Equal("atom.dsk", path);
            token.ThrowIfCancellationRequested();
            return Task.FromResult(data);
        });

        var image = await reader.ReadAsync("atom.dsk");
        Assert.Equal(DiskImageFormatIds.AcornAtomDos, image.FormatId);
        Assert.Equal(40, image.Cylinders);
        Assert.Equal(1, image.Heads);
        Assert.Equal(10, image.SectorsPerTrack);
        Assert.Equal(400, image.AvailableBlocks.Count);

        var volume = new BbcDfsFileSystemReader().Read(image);
        Assert.Equal("ATOMDISK", volume.Name);
        var entry = Assert.Single(volume.Entries);
        Assert.Equal("GAME", entry.Name);
        Assert.Equal(new byte[] { 0x41, 0x42, 0x43 }, entry.Content);

        data[256 + 6] = 0;
        data[256 + 7] = 0;
        var fixedCapacityVariant = await reader.ReadAsync("atom.dsk");
        Assert.Equal(AcornAtomDskReader.Capacity, fixedCapacityVariant.Capacity);
        Assert.Equal("GAME", Assert.Single(new BbcDfsFileSystemReader().Read(fixedCapacityVariant).Entries).Name);
        data[256 + 5] = 3;
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync("atom.dsk"));
    }

    public static void ProbeRejectsGenericDskWithoutAtomCatalog()
    {
        Assert.False(AcornAtomDskReader.LooksLikeAtomDos(new byte[512]));
        Assert.True(AcornAtomDskReader.LooksLikeAtomDos(CreateImage()));
    }

    private static byte[] CreateImage()
    {
        var data = new byte[AcornAtomDskReader.Capacity];
        "ATOMDISK"u8.CopyTo(data);
        "    "u8.CopyTo(data.AsSpan(256));
        "GAME   "u8.CopyTo(data.AsSpan(8));
        data[15] = (byte)' ';
        data[256 + 5] = 8;
        data[256 + 6] = 1;
        data[256 + 7] = 0x90;
        data[256 + 8 + 4] = 3;
        data[256 + 8 + 7] = 2;
        data[512] = 0x41;
        data[513] = 0x42;
        data[514] = 0x43;
        return data;
    }
}
