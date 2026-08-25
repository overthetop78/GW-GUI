using GWGUI.MediaEngine.Containers.Atari.St;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Atari;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie la détection BPB, les replis ordonnés et les erreurs Atari ST.</summary>
public sealed class AtariStGeometryTests
{
    /// <summary>Vérifie une géométrie issue d'un BPB cohérent.</summary>
    [Fact]
    public void DetectsGeometryFromBpb()
    {
        var data = CreateBpbImage(80, 2, 9);
        var detection = AtariStGeometryDetector.Detect(data);
        Assert.Equal(AtariStGeometryEvidence.Bpb, detection.Evidence);
        Assert.Equal(new AtariStGeometry(80, 2, 9), detection.Geometry);
    }

    /// <summary>Vérifie chaque nombre de secteurs et chaque nombre de faces lorsque la capacité produit des cylindres admissibles.</summary>
    [Theory]
    [InlineData(9, 2, 80, 2, 9)]
    [InlineData(9, 1, 40, 2, 9)]
    [InlineData(10, 2, 80, 2, 10)]
    [InlineData(10, 1, 40, 2, 10)]
    [InlineData(11, 2, 88, 2, 10)]
    [InlineData(11, 1, 44, 2, 10)]
    [InlineData(18, 2, 80, 2, 18)]
    [InlineData(18, 1, 80, 2, 9)]
    public void DetectsOrderedCapacityFallbacks(int sourceSectors, int sourceHeads, int expectedCylinders, int expectedHeads, int expectedSectors)
    {
        var data = new byte[80 * sourceHeads * sourceSectors * AtariStGeometry.SectorSize];
        var detection = AtariStGeometryDetector.Detect(data);
        Assert.Equal(AtariStGeometryEvidence.CapacityFallback, detection.Evidence);
        Assert.Equal(expectedCylinders, detection.Geometry.Cylinders);
        Assert.Equal(expectedHeads, detection.Geometry.Heads);
        Assert.Equal(expectedSectors, detection.Geometry.SectorsPerTrack);
        Assert.Equal(data.Length, detection.Geometry.Capacity);
    }

    /// <summary>Vérifie qu'un BPB incohérent laisse la capacité sélectionner un repli valide.</summary>
    [Fact]
    public void InvalidBpbFallsBackToCapacity()
    {
        var data = CreateBpbImage(80, 2, 9);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), 256);
        Assert.Equal(AtariStGeometryEvidence.CapacityFallback, AtariStGeometryDetector.Detect(data).Evidence);
    }

    /// <summary>Vérifie les longueurs vide, non sectorielle et sans géométrie admissible.</summary>
    [Fact]
    public async Task RejectsInvalidLengthsAndUnknownGeometry()
    {
        await AssertReaderRejects(Array.Empty<byte>());
        await AssertReaderRejects(new byte[AtariStGeometry.SectorSize + 1]);
        await AssertReaderRejects(new byte[34 * 7 * AtariStGeometry.SectorSize]);
    }

    /// <summary>Vérifie format, géométrie, adresses, capacité et contenu produits par le Reader public.</summary>
    [Fact]
    public async Task PublicReaderPreservesGeometryAddressesCapacityAndContent()
    {
        var data = CreateBpbImage(80, 2, 9);
        data[^1] = 0xA5;
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(path, data);
            var image = await new AtariStReader().ReadAsync(path);
            Assert.Equal("atarist.720", image.FormatId);
            Assert.Equal(80, image.Cylinders);
            Assert.Equal(2, image.Heads);
            Assert.Equal(9, image.SectorsPerTrack);
            Assert.Equal(data.Length, image.Capacity);
            Assert.Equal(new(0, 0, 1), image.AvailableBlocks.Single(block => block.LogicalBlock == 0).Address);
            var last = image.AvailableBlocks.Single(block => block.LogicalBlock == image.BlockCount - 1);
            Assert.Equal(new(79, 1, 9), last.Address);
            Assert.Equal(0xA5, last.Data[^1]);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Crée une image dont les champs BPB géométriques sont cohérents avec la capacité.</summary>
    private static byte[] CreateBpbImage(int cylinders, int heads, int sectors)
    {
        var totalSectors = cylinders * heads * sectors;
        var data = new byte[totalSectors * AtariStGeometry.SectorSize];
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), AtariStGeometry.SectorSize);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.TotalSectors16Offset), checked((ushort)totalSectors));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), checked((ushort)sectors));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.HeadCountOffset), checked((ushort)heads));
        return data;
    }

    /// <summary>Écrit une charge utile temporaire et vérifie son rejet par le Reader public.</summary>
    private static async Task AssertReaderRejects(byte[] data)
    {
        var path = Path.GetTempFileName();
        try { await File.WriteAllBytesAsync(path, data); await Assert.ThrowsAsync<InvalidDataException>(() => new AtariStReader().ReadAsync(path)); }
        finally { File.Delete(path); }
    }
}
