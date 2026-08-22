using GWGUI.MediaEngine.Containers.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Mfs;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie les composants et le lecteur public Macintosh MFS.</summary>
public sealed class MacMfsFileSystemTests
{
    /// <summary>Vérifie le décodage des entrées 12 bits paires, impaires et finales.</summary>
    [Fact]
    public void AllocationMapDecodesEvenOddAndFinalEntries()
    {
        var map = MacMfsAllocationMap.Decode([0x12, 0x34, 0x56, 0x78, 0x90], 3);
        Assert.Equal(0x123, map[0]);
        Assert.Equal(0x456, map[1]);
        Assert.Equal(0x789, map[2]);
        Assert.Throws<InvalidDataException>(() => MacMfsAllocationMap.Decode([0x12], 2));
    }

    /// <summary>Vérifie cycle, allocation hors carte et fin prématurée.</summary>
    [Fact]
    public void AllocationMapDistinguishesInvalidChains()
    {
        var cycle = MacMfsAllocationMap.Decode([0x00, 0x20], 1).Traverse(2, 2);
        Assert.True(cycle.HasCycle);
        var outOfRange = MacMfsAllocationMap.Decode([0x00, 0x20], 1).Traverse(3, 1);
        Assert.True(outOfRange.IsOutOfRange);
        var premature = MacMfsAllocationMap.Decode([0xff, 0x10], 1).Traverse(2, 2);
        Assert.True(premature.IsPrematureEnd);
    }

    /// <summary>Vérifie qu'un bloc absent ne décale pas le secteur suivant d'un fork.</summary>
    [Fact]
    public void ForkReaderPreservesMissingBlockPosition()
    {
        var map = MacMfsAllocationMap.Decode([0x00, 0x3f, 0xf1], 2);
        var image = new SectorImage(DiskImageFormatIds.AppleMacMfs, 512, 1, 1, 3, [Block(0, 0), Block(2, 0x22)]);
        var warnings = new List<string>();
        var result = MacMfsForkReader.Read(image, map, 1, 512, 2, 1024, "Fichier", MacMfsFileSystemLayout.DataForkName, warnings);
        Assert.False(result.IsValid);
        Assert.Equal([1], result.MissingBlocks);
        Assert.All(result.Content.Take(512), value => Assert.Equal(0, value));
        Assert.Equal(0x22, result.Content[512]);
    }

    /// <summary>Vérifie un bloc de répertoire absent et une longueur de nom invalide.</summary>
    [Fact]
    public void DirectoryReaderRejectsMissingBlockAndOversizedName()
    {
        var map = MacMfsAllocationMap.Decode([], 0);
        var missingImage = new SectorImage(DiskImageFormatIds.AppleMacMfs, 512, 1, 1, 2, [Block(0, 0)]);
        var missingWarnings = new List<string>();
        Assert.Empty(MacMfsDirectoryReader.Read(missingImage, 1, 1, map, 0, 512, missingWarnings));
        Assert.NotEmpty(missingWarnings);
        var invalidDirectory = new byte[512];
        invalidDirectory[MacMfsFileSystemLayout.FlagsOffset] = MacMfsFileSystemLayout.ActiveEntryMask;
        invalidDirectory[MacMfsFileSystemLayout.NameLengthOffset] = MacMfsFileSystemLayout.MaximumNameLength + 1;
        var invalidImage = new SectorImage(DiskImageFormatIds.AppleMacMfs, 512, 1, 1, 1, [Block(0, invalidDirectory)]);
        var invalidWarnings = new List<string>();
        Assert.Empty(MacMfsDirectoryReader.Read(invalidImage, 0, 1, map, 0, 512, invalidWarnings));
        Assert.Contains(invalidWarnings, warning => warning.Contains("offset 0", StringComparison.Ordinal));
    }

    /// <summary>Vérifie une taille d'allocation invalide et un nombre libre incohérent.</summary>
    [Fact]
    public void ReaderValidatesAllocationMetadata()
    {
        Assert.Throws<InvalidDataException>(() => new MacMfsFileSystemReader().Read(CreateMdbImage(513, 0)));
        var volume = new MacMfsFileSystemReader().Read(CreateMdbImage(512, 2));
        Assert.Equal(0, volume.FreeBytes);
        Assert.Contains(volume.Warnings, warning => warning.Contains("blocs libres", StringComparison.Ordinal));
    }

    /// <summary>Vérifie une signature incorrecte et un MDB tronqué.</summary>
    [Fact]
    public void ReaderRejectsInvalidSignatureAndTruncatedMdb()
    {
        var invalid = CreateMdbImage(512, 0, signature: 0);
        Assert.False(new MacMfsFileSystemReader().CanRead(invalid));
        var blocks = new[] { Block(0, 0), Block(1, 0), new SectorBlock(2, new(0, 0, 2), new byte[MacMfsFileSystemLayout.MinimumMdbLength - 1]) };
        var truncated = new SectorImage(DiskImageFormatIds.AppleMacMfs, 512, 1, 1, 3, blocks, allowVariableBlockSize: true);
        Assert.False(new MacMfsFileSystemReader().CanRead(truncated));
    }

    /// <summary>Vérifie par le lecteur public les images MFS 400 Kio disponibles.</summary>
    [Theory]
    [InlineData("*Macintosh System Disk 1.1g*.dsk")]
    [InlineData("*System 3.0.dsk")]
    [InlineData("*System 3.3.dsk")]
    public async Task PublicReaderReadsRealMfsImages(string pattern)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "validated_images", "Apple", "Macintosh"));
        var path = Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories).Single();
        var image = await new AppleDiskImageReader().ReadAsync(path);
        var volume = new MacMfsFileSystemReader().Read(image);
        Assert.Equal(409600, image.Capacity);
        Assert.Equal("mac-mfs", volume.FileSystemId);
        Assert.NotEmpty(volume.Name);
        Assert.NotEmpty(volume.Entries);
        Assert.All(volume.Entries, entry => Assert.NotNull(entry.Content));
    }

    /// <summary>Crée une image contenant deux blocs d'informations MFS.</summary>
    private static SectorImage CreateMdbImage(uint allocationSize, ushort freeAllocations, ushort signature = MacMfsFileSystemLayout.Signature)
    {
        var data = new byte[MacMfsFileSystemLayout.SectorSize * MacMfsFileSystemLayout.VolumeInformationBlockCount];
        BinaryPrimitives.WriteUInt16BigEndian(data, signature);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(MacMfsFileSystemLayout.AllocationCountOffset), 1);
        BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(MacMfsFileSystemLayout.AllocationSizeOffset), allocationSize);
        BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(MacMfsFileSystemLayout.FreeAllocationCountOffset), freeAllocations);
        data[MacMfsFileSystemLayout.VolumeNameOffset] = 3;
        "MFS"u8.CopyTo(data.AsSpan(MacMfsFileSystemLayout.VolumeNameOffset + 1));
        return new(DiskImageFormatIds.AppleMacMfs, 512, 1, 1, 4, [Block(0, 0), Block(1, 0), Block(2, data[..512]), Block(3, data[512..])]);
    }

    /// <summary>Crée un secteur rempli avec une valeur connue.</summary>
    private static SectorBlock Block(int logical, byte value) => Block(logical, Enumerable.Repeat(value, 512).Select(item => (byte)item).ToArray());

    /// <summary>Crée un secteur depuis son contenu.</summary>
    private static SectorBlock Block(int logical, byte[] data) => new(logical, new(0, 0, logical), data);
}
