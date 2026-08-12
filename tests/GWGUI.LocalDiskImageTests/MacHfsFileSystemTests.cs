using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les composants et le lecteur public Macintosh HFS.</summary>
public sealed class MacHfsFileSystemTests
{
    /// <summary>Vérifie les MDB invalides et une taille d'allocation non alignée.</summary>
    [Fact]
    public void ReaderRejectsInvalidMdbAndAllocationSize()
    {
        var invalidSignature = CreateMdbImage(0, MacHfsFileSystemLayout.SectorSize);
        Assert.False(new MacHfsFileSystemReader().CanRead(invalidSignature));
        var shortMdbBlocks = new[] { Block(0, 0), Block(1, 0), new SectorBlock(2, new(0, 0, 2), new byte[MacHfsFileSystemLayout.MinimumMdbLength - 1]) };
        var shortMdb = new SectorImage(DiskImageFormatIds.AppleMacHfs, 512, 1, 1, 3, shortMdbBlocks, allowVariableBlockSize: true);
        Assert.False(new MacHfsFileSystemReader().CanRead(shortMdb));
        var invalidAllocation = CreateMdbImage(MacHfsFileSystemLayout.Signature, 513);
        Assert.Throws<InvalidDataException>(() => new MacHfsFileSystemReader().Read(invalidAllocation));
    }

    /// <summary>Vérifie les trois extents intégrés et la conservation d'un bloc absent.</summary>
    [Fact]
    public void ExtentReaderPreservesMissingBlockPosition()
    {
        var blocks = new[] { Block(0, 0), Block(1, 0), Block(2, 0x11), Block(4, 0x33) };
        var image = new SectorImage(DiskImageFormatIds.AppleMacHfs, 512, 1, 1, 5, blocks);
        var extents = new byte[MacHfsFileSystemLayout.EmbeddedExtentsLength];
        for (var index = 0; index < MacHfsFileSystemLayout.EmbeddedExtentCount; index++)
        {
            var offset = index * MacHfsFileSystemLayout.ExtentDescriptorLength;
            BinaryPrimitives.WriteUInt16BigEndian(extents.AsSpan(offset + MacHfsFileSystemLayout.ExtentStartOffset), (ushort)index);
            BinaryPrimitives.WriteUInt16BigEndian(extents.AsSpan(offset + MacHfsFileSystemLayout.ExtentCountOffset), 1);
        }
        var result = MacHfsExtentReader.Read(image, extents, 2, 512, 1536);
        Assert.False(result.IsValid);
        Assert.Equal([3], result.MissingBlocks);
        Assert.Equal(0x11, result.Content[0]);
        Assert.All(result.Content.Skip(512).Take(512), value => Assert.Equal(0, value));
        Assert.Equal(0x33, result.Content[1024]);
    }

    /// <summary>Vérifie le besoin d'extents supplémentaires et une taille sectorielle invalide.</summary>
    [Fact]
    public void ExtentReaderReportsRemainingLengthAndInvalidAllocation()
    {
        var image = new SectorImage(DiskImageFormatIds.AppleMacHfs, 512, 1, 1, 1, [Block(0, 0)]);
        var result = MacHfsExtentReader.Read(image, new byte[MacHfsFileSystemLayout.EmbeddedExtentsLength], 0, 512, 512);
        Assert.Equal(512, result.RemainingLength);
        Assert.Throws<InvalidDataException>(() => MacHfsExtentReader.Read(image, new byte[MacHfsFileSystemLayout.EmbeddedExtentsLength], 0, 513, 512));
    }

    /// <summary>Vérifie un catalogue tronqué et l'absence de record lisible.</summary>
    [Fact]
    public void CatalogReaderReportsTruncatedAndEmptyCatalog()
    {
        var image = new SectorImage(DiskImageFormatIds.AppleMacHfs, 512, 1, 1, 1, [Block(0, 0)]);
        Assert.Throws<InvalidDataException>(() => MacHfsCatalogReader.Read(new byte[MacHfsFileSystemLayout.MinimumCatalogLength - 1], image, 0, 512, []));
        var warnings = new List<string>();
        MacHfsCatalogReader.Read(new byte[MacHfsFileSystemLayout.DefaultNodeSize], image, 0, 512, warnings);
        Assert.Contains(warnings, warning => warning.Contains("aucun record lisible", StringComparison.Ordinal));
    }

    /// <summary>Vérifie un nœud non-feuille, une table d'offsets invalide et un fichier possédant deux forks.</summary>
    [Fact]
    public void CatalogReaderValidatesNodesAndKeepsBothForks()
    {
        var image = new SectorImage(DiskImageFormatIds.AppleMacHfs, 512, 1, 1, 2, [Block(0, 0x11), Block(1, 0x22)]);
        var nonLeaf = CatalogWithLeafNode(false, validOffsets: true, includeFile: false);
        Assert.Empty(MacHfsCatalogReader.Read(nonLeaf, image, 0, 512, []));
        var invalidOffsets = CatalogWithLeafNode(true, validOffsets: false, includeFile: false);
        Assert.Empty(MacHfsCatalogReader.Read(invalidOffsets, image, 0, 512, []));
        var valid = CatalogWithLeafNode(true, validOffsets: true, includeFile: true);
        var record = Assert.Single(MacHfsCatalogReader.Read(valid, image, 0, 512, []));
        Assert.Equal(512, record.DataFork.Count);
        Assert.Equal(512, record.ResourceFork.Count);
        Assert.True(record.IsValid);
    }

    /// <summary>Vérifie la détection d'un cycle et la propagation de la validité d'un fichier.</summary>
    [Fact]
    public void DirectoryBuilderReportsCycleAndPropagatesValidity()
    {
        MacHfsCatalogRecord[] records =
        [
            new(2, 3, "Dossier", true, 0, null, "Dossier", [], [], true),
            new(3, 3, "Cycle", true, 0, null, "Dossier", [], [], true),
            new(2, 4, "Fichier", false, 1, null, "TEXT", [1], [2], false)
        ];
        var warnings = new List<string>();
        var entries = MacHfsDirectoryBuilder.Build(records, warnings);
        Assert.Contains(warnings, warning => warning.Contains("forme un cycle", StringComparison.Ordinal));
        Assert.False(entries.Single(entry => entry.Name == "Fichier").MetadataValid);
    }

    /// <summary>Vérifie par le lecteur public les images HFS 800 Kio et 1,44 Mio disponibles.</summary>
    [Theory]
    [InlineData("*System 3.2.dsk", 819200)]
    [InlineData("*System_6.0.6_System_Startup.dsk", 1474560)]
    public async Task PublicReaderReadsRealHfsImages(string pattern, long expectedCapacity)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "Apple", "Macintosh"));
        var path = Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories).Single();
        var image = await new AppleDiskImageReader().ReadAsync(path);
        var volume = new MacHfsFileSystemReader().Read(image);
        Assert.Equal(expectedCapacity, image.Capacity);
        Assert.Equal("mac-hfs", volume.FileSystemId);
        Assert.NotEmpty(volume.Name);
        Assert.NotEmpty(volume.Entries);
        Assert.NotNull(volume.Created);
    }

    /// <summary>Crée une image contenant un MDB minimal.</summary>
    private static SectorImage CreateMdbImage(ushort signature, uint allocationSize)
    {
        var blocks = new[] { Block(0, 0), Block(1, 0), Block(2, 0), Block(3, 0) };
        var mdb = blocks[2].Data.ToArray();
        BinaryPrimitives.WriteUInt16BigEndian(mdb, signature);
        BinaryPrimitives.WriteUInt16BigEndian(mdb.AsSpan(MacHfsFileSystemLayout.AllocationCountOffset), 1);
        BinaryPrimitives.WriteUInt32BigEndian(mdb.AsSpan(MacHfsFileSystemLayout.AllocationSizeOffset), allocationSize);
        return new(DiskImageFormatIds.AppleMacHfs, 512, 1, 1, blocks.Length, [blocks[0], blocks[1], blocks[2] with { Data = mdb }, blocks[3]]);
    }

    /// <summary>Crée un catalogue contenant un nœud paramétrable.</summary>
    private static byte[] CatalogWithLeafNode(bool leaf, bool validOffsets, bool includeFile)
    {
        var catalog = new byte[MacHfsFileSystemLayout.DefaultNodeSize * 2];
        BinaryPrimitives.WriteUInt16BigEndian(catalog.AsSpan(MacHfsFileSystemLayout.NodeSizeOffset), MacHfsFileSystemLayout.DefaultNodeSize);
        var node = catalog.AsSpan(MacHfsFileSystemLayout.DefaultNodeSize, MacHfsFileSystemLayout.DefaultNodeSize);
        node[MacHfsFileSystemLayout.NodeKindOffset] = leaf ? unchecked((byte)MacHfsFileSystemLayout.LeafNodeKind) : (byte)0;
        BinaryPrimitives.WriteUInt16BigEndian(node.Slice(MacHfsFileSystemLayout.RecordCountOffset), 1);
        var start = MacHfsFileSystemLayout.NodeDescriptorLength;
        var end = includeFile ? 128 : 32;
        BinaryPrimitives.WriteUInt16BigEndian(node[^2..], validOffsets ? (ushort)start : (ushort)200);
        BinaryPrimitives.WriteUInt16BigEndian(node[^4..], validOffsets ? (ushort)end : (ushort)100);
        if (!includeFile) return catalog;
        node[start] = 7;
        BinaryPrimitives.WriteUInt32BigEndian(node.Slice(start + 2), MacHfsFileSystemLayout.RootDirectoryId);
        node[start + 6] = 1;
        node[start + 7] = (byte)'F';
        var dataOffset = start + 8;
        node[dataOffset] = MacHfsFileSystemLayout.FileRecordType;
        BinaryPrimitives.WriteUInt32BigEndian(node.Slice(dataOffset + MacHfsFileSystemLayout.FileIdOffset), 5);
        BinaryPrimitives.WriteUInt32BigEndian(node.Slice(dataOffset + MacHfsFileSystemLayout.DataForkLengthOffset), 512);
        BinaryPrimitives.WriteUInt32BigEndian(node.Slice(dataOffset + MacHfsFileSystemLayout.ResourceForkLengthOffset), 512);
        BinaryPrimitives.WriteUInt16BigEndian(node.Slice(dataOffset + MacHfsFileSystemLayout.DataForkExtentsOffset + MacHfsFileSystemLayout.ExtentCountOffset), 1);
        BinaryPrimitives.WriteUInt16BigEndian(node.Slice(dataOffset + MacHfsFileSystemLayout.ResourceForkExtentsOffset + MacHfsFileSystemLayout.ExtentStartOffset), 1);
        BinaryPrimitives.WriteUInt16BigEndian(node.Slice(dataOffset + MacHfsFileSystemLayout.ResourceForkExtentsOffset + MacHfsFileSystemLayout.ExtentCountOffset), 1);
        return catalog;
    }

    /// <summary>Crée un secteur rempli avec une valeur connue.</summary>
    private static SectorBlock Block(int logical, byte value) => new(logical, new(0, 0, logical), Enumerable.Repeat(value, 512).Select(item => (byte)item).ToArray());
}
