using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.ProDos;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les composants du lecteur ProDOS et SOS.</summary>
public sealed class ProDosFileSystemTests
{
    /// <summary>Vérifie les fichiers seedling, sapling et tree avec un bloc creux intermédiaire.</summary>
    [Theory]
    [InlineData((byte)ProDosStorageType.Seedling)]
    [InlineData((byte)ProDosStorageType.Sapling)]
    [InlineData((byte)ProDosStorageType.Tree)]
    public void ContentReaderPreservesLogicalBlockPositions(byte storageValue)
    {
        var storageType = (ProDosStorageType)storageValue;
        var blocks = Enumerable.Range(0, 6).Select(index => Block(index, (byte)(0x10 + index))).ToArray();
        var keyBlock = storageType == ProDosStorageType.Seedling ? 3 : 1;
        var length = storageType == ProDosStorageType.Seedling ? 512 : 1536;
        if (storageType == ProDosStorageType.Sapling)
        {
            var index = new byte[ProDosFileSystemLayout.BlockSize];
            WritePointer(index, 0, 3);
            WritePointer(index, 2, 4);
            blocks[1] = blocks[1] with { Data = index };
        }
        else if (storageType == ProDosStorageType.Tree)
        {
            var master = new byte[ProDosFileSystemLayout.BlockSize];
            WritePointer(master, 0, 2);
            blocks[1] = blocks[1] with { Data = master };
            var index = new byte[ProDosFileSystemLayout.BlockSize];
            WritePointer(index, 0, 3);
            WritePointer(index, 2, 4);
            blocks[2] = blocks[2] with { Data = index };
        }
        var image = new SectorImage(DiskImageFormatIds.AppleIIProDos, 512, 1, 1, blocks.Length, blocks);
        var result = ProDosFileContentReader.Read(image, storageType, keyBlock, length, "TEST", []);
        Assert.True(result.IsValid);
        Assert.Equal(0x13, result.Content[0]);
        if (length > 512)
        {
            Assert.All(result.Content.Skip(512).Take(512), value => Assert.Equal(0, value));
            Assert.Equal(0x14, result.Content[1024]);
        }
    }

    /// <summary>Vérifie les blocs d'index et de données absents ou hors image.</summary>
    [Fact]
    public void ContentReaderReportsInvalidIndexAndDataBlocks()
    {
        var image = new SectorImage(DiskImageFormatIds.AppleIIProDos, 512, 1, 1, 3, [Block(0, 0), Block(1, 0), Block(2, 0)]);
        Assert.False(ProDosFileContentReader.Read(image, ProDosStorageType.Sapling, 9, 512, "INDEX", []).IsValid);
        Assert.False(ProDosFileContentReader.Read(image, ProDosStorageType.Seedling, 9, 512, "DATA", []).IsValid);
        Assert.False(ProDosFileContentReader.Read(image, ProDosStorageType.Tree, 9, 512, "MASTER", []).IsValid);
    }

    /// <summary>Vérifie le bitmap valide, absent et tronqué.</summary>
    [Fact]
    public void BitmapReaderReportsItsValidity()
    {
        var bitmap = Block(1, 0xff);
        var validImage = new SectorImage(DiskImageFormatIds.AppleIIProDos, 512, 1, 1, 2, [Block(0, 0), bitmap]);
        var valid = ProDosBitmapReader.Read(validImage, 1, 2, []);
        Assert.True(valid.IsValid);
        Assert.Equal(2, valid.FreeBlocks);
        var absentImage = new SectorImage(DiskImageFormatIds.AppleIIProDos, 512, 1, 1, 2, [Block(0, 0)]);
        Assert.False(ProDosBitmapReader.Read(absentImage, 1, 2, []).IsValid);
        var truncatedImage = new SectorImage(DiskImageFormatIds.AppleIIProDos, 512, 1, 1, 2, [Block(0, 0), new SectorBlock(1, new(0, 0, 1), new byte[10])], allowVariableBlockSize: true);
        Assert.False(ProDosBitmapReader.Read(truncatedImage, 1, 2, []).IsValid);
    }

    /// <summary>Vérifie les types connus, inconnu et les dates valides ou impossibles.</summary>
    [Fact]
    public void DefinitionsDecodeTypesAndDates()
    {
        Assert.Equal("Texte", ProDosFileTypeNames.Get(ProDosFileType.Text));
        Assert.Equal("Binaire", ProDosFileTypeNames.Get(ProDosFileType.Binary));
        Assert.Equal("Répertoire", ProDosFileTypeNames.Get(ProDosFileType.Directory));
        Assert.Contains("$42", ProDosFileTypeNames.Get((ProDosFileType)0x42));
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, (ushort)((84 << ProDosDateTime.YearShift) | (1 << ProDosDateTime.MonthShift) | 1));
        Assert.Equal(1984, ProDosDateTime.Read(bytes, 0)!.Value.Year);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, 0);
        Assert.Null(ProDosDateTime.Read(bytes, 0));
    }

    /// <summary>Vérifie l'en-tête de volume complet et un total de blocs incohérent.</summary>
    [Fact]
    public void ReaderValidatesVolumeHeaderAndDeclaredCapacity()
    {
        var image = CreateVolume(totalBlocks: 10);
        var volume = new ProDosFileSystemReader().Read(image);
        Assert.Equal("TEST", volume.Name);
        Assert.Contains(volume.Warnings, warning => warning.Contains("annonce 10 blocs", StringComparison.Ordinal));
        var invalid = image.GetBlock(ProDosFileSystemLayout.RootBlock).ToArray();
        invalid[ProDosFileSystemLayout.HeaderEntryLengthOffset] = 0;
        Assert.False(ProDosVolumeHeaderReader.TryRead(invalid, out _));
    }

    /// <summary>Vérifie un cycle, un bloc absent et la profondeur maximale d'un répertoire.</summary>
    [Fact]
    public void DirectoryReaderReportsInvalidChainsAndDepth()
    {
        var cyclicBlock = Block(2, 0);
        var bytes = cyclicBlock.Data.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(ProDosFileSystemLayout.NextBlockOffset), 2);
        var image = new SectorImage(DiskImageFormatIds.AppleIIProDos, 512, 1, 1, 3, [Block(0, 0), Block(1, 0), cyclicBlock with { Data = bytes }]);
        var cycleWarnings = new List<string>();
        Assert.False(ProDosDirectoryReader.Read(image, 2, cycleWarnings, new HashSet<int>(), 0).IsValid);
        Assert.Contains(cycleWarnings, warning => warning.Contains("cycle=True", StringComparison.Ordinal));
        Assert.False(ProDosDirectoryReader.Read(image, 9, [], new HashSet<int>(), 0).IsValid);
        Assert.False(ProDosDirectoryReader.Read(image, 2, [], new HashSet<int>(), ProDosFileSystemLayout.MaximumDirectoryDepth + 1).IsValid);
    }

    /// <summary>Vérifie par les lecteurs publics une image ProDOS 140 Kio, une image ProDOS 800 Kio et une image Apple III SOS.</summary>
    [Theory]
    [InlineData("Apple II", "*Beagle Graphics*ProDOS*.woz")]
    [InlineData("Apple II", "AMR35SCS.po")]
    [InlineData("Apple III", "Backup3.dsk")]
    public async Task PublicReaderReadsRealProDosAndSosImages(string directory, string pattern)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", directory));
        var path = Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories).Single();
        var image = await new AppleDiskImageReader().ReadAsync(path);
        var volume = new ProDosFileSystemReader().Read(image);
        Assert.Equal("prodos", volume.FileSystemId);
        Assert.NotEmpty(volume.Name);
        Assert.NotEmpty(volume.Entries);
        Assert.True(volume.Capacity > 0);
    }

    /// <summary>Crée un volume ProDOS minimal.</summary>
    private static SectorImage CreateVolume(int totalBlocks)
    {
        var blocks = Enumerable.Range(0, 5).Select(index => Block(index, 0)).ToArray();
        var root = blocks[2].Data.ToArray();
        root[ProDosFileSystemLayout.HeaderOffset] = (byte)(((int)ProDosStorageType.VolumeHeader << ProDosFileSystemLayout.StorageTypeShift) | 4);
        "TEST"u8.CopyTo(root.AsSpan(ProDosFileSystemLayout.HeaderOffset + ProDosFileSystemLayout.NameOffset));
        root[ProDosFileSystemLayout.HeaderEntryLengthOffset] = ProDosFileSystemLayout.EntrySize;
        BinaryPrimitives.WriteUInt16LittleEndian(root.AsSpan(ProDosFileSystemLayout.BitmapBlockOffset), 4);
        BinaryPrimitives.WriteUInt16LittleEndian(root.AsSpan(ProDosFileSystemLayout.TotalBlocksOffset), (ushort)totalBlocks);
        blocks[2] = blocks[2] with { Data = root };
        blocks[4] = Block(4, 0xff);
        return new(DiskImageFormatIds.AppleIIProDos, 512, 1, 1, blocks.Length, blocks);
    }

    /// <summary>Écrit un pointeur dans les deux moitiés d'un bloc d'index.</summary>
    private static void WritePointer(byte[] index, int position, int block)
    {
        index[position] = (byte)block;
        index[position + ProDosFileSystemLayout.IndexHighBytesOffset] = (byte)(block >> ProDosFileSystemLayout.BitsPerByte);
    }

    /// <summary>Crée un bloc rempli d'une valeur connue.</summary>
    private static SectorBlock Block(int logical, byte value) => new(logical, new(0, 0, logical), Enumerable.Repeat(value, 512).Select(item => (byte)item).ToArray());
}
