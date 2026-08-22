using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Lisa;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions et la lecture publique du système de fichiers Lisa.</summary>
public sealed class LisaFileSystemDefinitionsTests
{
    /// <summary>Vérifie les trois dispositions de catalogue prises en charge.</summary>
    [Theory]
    [InlineData(LisaCatalogVersion.Table, LisaFileSystemLayout.TableEntrySize, 0)]
    [InlineData(LisaCatalogVersion.Hash, LisaFileSystemLayout.TreeEntrySize, LisaFileSystemLayout.TreeEntriesOffset)]
    [InlineData(LisaCatalogVersion.BTree, LisaFileSystemLayout.TreeEntrySize, LisaFileSystemLayout.TreeEntriesOffset)]
    public void PublicReaderReadsEverySupportedCatalog(LisaCatalogVersion version, int entrySize, int entryOffset)
    {
        var volume = new LisaFileSystemReader().Read(CreateImage((ushort)version, entrySize, entryOffset, includeCatalog: true, pages: [0, 1]));
        Assert.Equal("VOLUME", volume.Name);
        Assert.Equal("DOCUMENT", Assert.Single(volume.Entries).Name);
        Assert.Equal(0, volume.FreeBytes);
    }

    /// <summary>Vérifie les tags valides, absents, tronqués, libres et réservés.</summary>
    [Fact]
    public void TagReaderDistinguishesEveryTagState()
    {
        Assert.True(LisaPageTagReader.TryRead(Block(0, 5, 3), out var valid));
        Assert.Equal(new LisaPageTag(5, 3), valid);
        Assert.False(LisaPageTagReader.TryRead(Block(1, 5, 0) with { Tag = null }, out _));
        Assert.False(LisaPageTagReader.TryRead(Block(2, 5, 0) with { Tag = new byte[LisaFileSystemLayout.TagLength - 1] }, out _));
        Assert.False(LisaPageTagReader.IsUserFile(LisaFileSystemLayout.MddfFileId));
        Assert.False(LisaPageTagReader.IsUserFile(LisaFileSystemLayout.ReservedFileIds.First()));
        Assert.True(LisaPageTagReader.IsUserFile(5));
        var freeBlocks = new[] { Block(0, LisaFileSystemLayout.FreePageFileId, 0), Block(1, LisaFileSystemLayout.AlternateFreePageFileId, 0) };
        Assert.All(freeBlocks, block => Assert.True(LisaPageTagReader.TryRead(block, out var tag) && tag.FileId is LisaFileSystemLayout.FreePageFileId or LisaFileSystemLayout.AlternateFreePageFileId));
    }

    /// <summary>Vérifie l'ordre des pages et la conservation d'une lacune dans le contenu.</summary>
    [Fact]
    public void ReaderOrdersPagesAndPreservesMissingPagePosition()
    {
        var image = CreateImage((ushort)LisaCatalogVersion.Table, LisaFileSystemLayout.TableEntrySize, 0, includeCatalog: true, pages: [2, 0]);
        var volume = new LisaFileSystemReader().Read(image);
        var entry = Assert.Single(volume.Entries);
        Assert.Equal(3 * image.BlockSize, entry.Content!.Count);
        Assert.All(entry.Content.Skip(image.BlockSize).Take(image.BlockSize), value => Assert.Equal(0, value));
        Assert.False(entry.MetadataValid);
        Assert.Contains(volume.Warnings, warning => warning.Contains("page 1", StringComparison.Ordinal));
    }

    /// <summary>Vérifie qu'une page dupliquée est signalée et n'est pas concaténée deux fois.</summary>
    [Fact]
    public void ReaderRejectsDuplicatePageInReconstructedContent()
    {
        var image = CreateImage((ushort)LisaCatalogVersion.Table, LisaFileSystemLayout.TableEntrySize, 0, includeCatalog: true, pages: [0, 0]);
        var volume = new LisaFileSystemReader().Read(image);
        Assert.Equal(image.BlockSize, Assert.Single(volume.Entries).Content!.Count);
        Assert.Contains(volume.Warnings, warning => warning.Contains("plusieurs pages 0", StringComparison.Ordinal));
    }

    /// <summary>Vérifie les noms de secours et l'avertissement lorsqu'aucun catalogue n'est disponible.</summary>
    [Fact]
    public void ReaderReportsMissingCatalogAndUsesFallbackName()
    {
        var volume = new LisaFileSystemReader().Read(CreateImage((ushort)LisaCatalogVersion.Table, 0, 0, includeCatalog: false, pages: [0]));
        Assert.Equal("Fichier 0005", Assert.Single(volume.Entries).Name);
        Assert.Contains(volume.Warnings, warning => warning.Contains("catalogue Lisa", StringComparison.Ordinal));
    }

    /// <summary>Vérifie qu'un MDDF tronqué et une version inconnue restent explicitement observables.</summary>
    [Fact]
    public void ReaderReportsTruncatedMddfAndUnknownVersion()
    {
        var truncated = new SectorImage(DiskImageFormatIds.AppleLisaOffice, 32, 1, 1, 1, [Block(0, LisaFileSystemLayout.MddfFileId, 0, 32)]);
        Assert.Throws<InvalidDataException>(() => new LisaFileSystemReader().Read(truncated));
        var unknown = new LisaFileSystemReader().Read(CreateImage(0x1234, LisaFileSystemLayout.TreeEntrySize, LisaFileSystemLayout.TreeEntriesOffset, includeCatalog: false, pages: [0]));
        Assert.Contains(unknown.Warnings, warning => warning.Contains("inconnue-0x1234", StringComparison.Ordinal));
    }

    /// <summary>Vérifie qu'un tag absent n'est ni une page libre ni un fichier inventé.</summary>
    [Fact]
    public void ReaderIgnoresBlocksWithoutValidTag()
    {
        var image = CreateImage((ushort)LisaCatalogVersion.Table, 0, 0, includeCatalog: false, pages: [0]);
        var blocks = image.AvailableBlocks.Concat([Block(image.AvailableBlocks.Count, LisaFileSystemLayout.FreePageFileId, 0) with { Tag = null }]).ToArray();
        var volume = new LisaFileSystemReader().Read(new(image.FormatId, image.BlockSize, 1, 1, blocks.Length, blocks));
        Assert.Equal(0, volume.FreeBytes);
        Assert.Contains(volume.Warnings, warning => warning.Contains("tag de 0 octet", StringComparison.Ordinal));
    }

    /// <summary>Vérifie de bout en bout une image Lisa réelle par les lecteurs publics.</summary>
    [Fact]
    public async Task PublicReaderReadsRealLisaImage()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
        var path = Directory.EnumerateFiles(root, "*LisaGuide.image", SearchOption.AllDirectories).Single();
        var image = await new AppleDiskImageReader().ReadAsync(path);
        var volume = new LisaFileSystemReader().Read(image);
        Assert.Equal(DiskImageFormatIds.AppleLisaOffice, image.FormatId);
        Assert.Equal("lisa", volume.FileSystemId);
        Assert.NotEmpty(volume.Name);
        Assert.NotEmpty(volume.Entries);
        Assert.All(volume.Entries, entry => Assert.NotNull(entry.Content));
    }

    /// <summary>Crée une image Lisa synthétique dont le contenu attendu est connu.</summary>
    private static SectorImage CreateImage(ushort version, int entrySize, int entryOffset, bool includeCatalog, int[] pages)
    {
        var mddf = Block(0, LisaFileSystemLayout.MddfFileId, 0);
        var mddfData = mddf.Data.ToArray();
        BinaryPrimitives.WriteUInt16BigEndian(mddfData, version);
        mddfData[LisaVolumeHeader.NameLengthOffset] = 6;
        "VOLUME"u8.CopyTo(mddfData.AsSpan(LisaVolumeHeader.NameOffset));
        var blocks = new List<SectorBlock> { mddf with { Data = mddfData } };
        if (includeCatalog)
        {
            var catalog = Block(1, LisaFileSystemLayout.CatalogFileId, 0);
            var data = catalog.Data.ToArray();
            if ((LisaCatalogVersion)version == LisaCatalogVersion.Table) data[entryOffset] = 8;
            else data[entryOffset + LisaFileSystemLayout.CatalogNameOffset] = (byte)'D';
            "DOCUMENT"u8.CopyTo(data.AsSpan(entryOffset + LisaFileSystemLayout.CatalogNameOffset));
            BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(entryOffset + LisaFileSystemLayout.CatalogFileIdOffset), 5);
            blocks.Add(catalog with { Data = data });
        }
        foreach (var page in pages) blocks.Add(Block(blocks.Count, 5, page));
        return new(DiskImageFormatIds.AppleLisaOffice, 512, 1, 1, blocks.Count, blocks);
    }

    /// <summary>Crée un bloc Lisa tagué.</summary>
    private static SectorBlock Block(int logical, ushort fileId, int page, int dataLength = 512)
    {
        var tag = new byte[LisaFileSystemLayout.TagLength];
        tag[LisaFileSystemLayout.TagFileIdHighOffset] = (byte)(fileId >> 8);
        tag[LisaFileSystemLayout.TagFileIdLowOffset] = (byte)fileId;
        tag[LisaFileSystemLayout.TagPageHighOffset] = (byte)(page >> 8);
        tag[LisaFileSystemLayout.TagPageLowOffset] = (byte)page;
        return new(logical, new(0, 0, logical), new byte[dataLength], Tag: tag);
    }
}
