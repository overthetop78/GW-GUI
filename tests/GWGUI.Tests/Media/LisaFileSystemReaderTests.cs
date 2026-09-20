using System.Text;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaFileSystems.Definitions;

namespace GWGUI.Tests.Media;

public sealed class LisaFileSystemReaderTests
{
    [Fact]
    public void ExplorerShowsOnlyNamesStoredInLisaCatalog()
    {
        var mddf = new byte[512];
        mddf[1] = 0x0e;
        mddf[12] = 4;
        Encoding.ASCII.GetBytes("TEST").CopyTo(mddf, 13);

        var catalog = new byte[512];
        catalog[0] = 5;
        Encoding.ASCII.GetBytes("HELLO").CopyTo(catalog, 1);
        catalog[37] = 5;

        var image = new SectorImage(DiskImageFormatIds.AppleLisaOffice, 512, 1, 1, 4,
        [
            Block(0, 1, mddf),
            Block(1, 4, catalog),
            Block(2, 5, [1, 2, 3]),
            Block(3, 6, [4, 5, 6])
        ], allowVariableBlockSize: true);

        var reader = Assert.Single(GWGUI.MediaFileSystems.Exploration.FileSystemReaderCatalog.CreateDefault(), item => item.Id == FileSystemIds.Lisa);
        Assert.True(reader.CanRead(image));
        var entry = Assert.Single(reader.Read(image).Entries);
        Assert.Equal("HELLO", entry.Name);
        var content = Assert.IsAssignableFrom<IReadOnlyList<byte>>(entry.Content);
        Assert.Equal(new byte[] { 1, 2, 3 }, content.Take(3));
    }

    private static SectorBlock Block(int logicalBlock, ushort fileId, IReadOnlyList<byte> data)
    {
        var tag = new byte[12];
        tag[4] = (byte)(fileId >> 8);
        tag[5] = (byte)fileId;
        return new(logicalBlock, new SectorAddress(0, 0, logicalBlock), data, Tag: tag);
    }
}
