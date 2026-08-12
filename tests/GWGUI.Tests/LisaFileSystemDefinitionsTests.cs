using System.IO;
using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Lisa;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class LisaFileSystemDefinitionsTests
{
    [Theory]
    [InlineData(LisaVolumeHeader.TableCatalogVersion, LisaFileSystemLayout.TableEntrySize, 0)]
    [InlineData(LisaVolumeHeader.HashCatalogVersion, LisaFileSystemLayout.TreeEntrySize, LisaFileSystemLayout.TreeEntriesOffset)]
    [InlineData(LisaVolumeHeader.BTreeCatalogVersion, LisaFileSystemLayout.TreeEntrySize, LisaFileSystemLayout.TreeEntriesOffset)]
    public void PublicReaderReadsEverySupportedCatalog(ushort version, int entrySize, int entryOffset)
    {
        var image = CreateImage(version, entrySize, entryOffset, includeCatalog: true, skipPage: false);
        var volume = new LisaFileSystemReader().Read(image);
        Assert.Equal("VOLUME", volume.Name);
        Assert.Equal("DOCUMENT", Assert.Single(volume.Entries).Name);
        Assert.Equal(0, volume.FreeBytes);
    }

    [Fact]
    public void ReaderOrdersPagesAndReportsMissingCatalogAndPage()
    {
        var missingCatalog = new LisaFileSystemReader().Read(CreateImage(LisaVolumeHeader.TableCatalogVersion, 0, 0, includeCatalog: false, skipPage: false));
        Assert.Contains(missingCatalog.Warnings, warning => warning.Contains("catalog pages are missing", StringComparison.Ordinal));
        Assert.StartsWith("File ", Assert.Single(missingCatalog.Entries).Name);
        var missingPage = new LisaFileSystemReader().Read(CreateImage(LisaVolumeHeader.TableCatalogVersion, LisaFileSystemLayout.TableEntrySize, 0, includeCatalog: true, skipPage: true));
        Assert.Contains(missingPage.Warnings, warning => warning.Contains("missing page 1", StringComparison.Ordinal));
    }

    [Fact]
    public void ReaderRejectsAnImageWithoutMddf()
    {
        var image = new SectorImage(DiskImageFormatIds.AppleLisaOffice, 512, 1, 1, 1, [Block(0, LisaFileSystemLayout.FreePageFileId, 0)]);
        Assert.Throws<InvalidDataException>(() => new LisaFileSystemReader().Read(image));
    }

    private static SectorImage CreateImage(ushort version, int entrySize, int entryOffset, bool includeCatalog, bool skipPage)
    {
        var mddf = Block(0, LisaFileSystemLayout.MddfFileId, 0);
        var mddfData = mddf.Data.ToArray();
        BinaryPrimitives.WriteUInt16BigEndian(mddfData, version);
        mddfData[LisaVolumeHeader.NameLengthOffset] = 6;
        "VOLUME"u8.CopyTo(mddfData.AsSpan(LisaVolumeHeader.NameOffset));
        mddf = mddf with { Data = mddfData };
        var blocks = new List<SectorBlock> { mddf };
        if (includeCatalog)
        {
            var catalog = Block(1, LisaFileSystemLayout.CatalogFileId, 0);
            var data = catalog.Data.ToArray();
            if (version == LisaVolumeHeader.TableCatalogVersion) data[entryOffset] = 8;
            else data[entryOffset + LisaFileSystemLayout.CatalogNameOffset] = (byte)'D';
            "DOCUMENT"u8.CopyTo(data.AsSpan(entryOffset + LisaFileSystemLayout.CatalogNameOffset));
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(entryOffset + LisaFileSystemLayout.CatalogFileIdOffset), 5);
            blocks.Add(catalog with { Data = data });
        }
        blocks.Add(Block(blocks.Count, 5, 0));
        blocks.Add(Block(blocks.Count, 5, skipPage ? 2 : 1));
        return new SectorImage(DiskImageFormatIds.AppleLisaOffice, 512, 1, 1, blocks.Count, blocks);
    }

    private static SectorBlock Block(int logical, ushort fileId, int page)
    {
        var tag = new byte[12];
        tag[LisaFileSystemLayout.TagFileIdHighOffset] = (byte)(fileId >> 8);
        tag[LisaFileSystemLayout.TagFileIdLowOffset] = (byte)fileId;
        tag[LisaFileSystemLayout.TagPageHighOffset] = (byte)(page >> 8);
        tag[LisaFileSystemLayout.TagPageLowOffset] = (byte)page;
        return new(logical, new(0, 0, logical), new byte[512], Tag: tag);
    }
}
