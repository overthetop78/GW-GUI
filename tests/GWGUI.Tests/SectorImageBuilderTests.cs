using System.IO;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders;
using GWGUI.MediaEngine.SectorImages.Builders.Apple;

namespace GWGUI.Tests;

/// <summary>Vérifie les builders linéaires, Macintosh GCR, Apple II et RWTS18.</summary>
public sealed class SectorImageBuilderTests
{
    /// <summary>Vérifie les bases zéro et un ainsi que le rejet d'une longueur incompatible.</summary>
    [Fact]
    public void LinearBuilderHonorsSectorNumberingAndLength()
    {
        var zeroGeometry = new LinearSectorImageGeometry(2, 1, 1, 2);
        var zero = LinearSectorImageBuilder.Create(new byte[4], "zero", zeroGeometry);
        Assert.Equal([0, 1], zero.AvailableBlocks.OrderBy(block => block.LogicalBlock).Select(block => block.Address.Number));
        var oneGeometry = new LinearSectorImageGeometry(2, 1, 1, 2, SectorNumbering.OneBased);
        var one = LinearSectorImageBuilder.Create(new byte[4], "one", oneGeometry);
        Assert.Equal([1, 2], one.AvailableBlocks.OrderBy(block => block.LogicalBlock).Select(block => block.Address.Number));
        Assert.Throws<InvalidDataException>(() => LinearSectorImageBuilder.Create(new byte[3], "bad", zeroGeometry));
    }

    /// <summary>Vérifie les premiers, changements de zone et derniers blocs Macintosh GCR.</summary>
    [Fact]
    public void MacintoshGcrBuilderUsesZonedAddresses()
    {
        var geometry = MacintoshGcrGeometry.ForHeads(MacintoshGcrGeometry.DoubleSidedHeadCount);
        var image = MacintoshGcrSectorImageBuilder.Create(new byte[geometry.Capacity], "mac", geometry);
        Assert.Equal(MacintoshGcrGeometry.Address(0, geometry.Heads), image.GetAddress(0));
        var zoneChange = MacintoshGcrGeometry.MaximumSectorsPerTrack * geometry.Heads * MacintoshGcrGeometry.ZoneCylinderCount;
        Assert.Equal(MacintoshGcrGeometry.Address(zoneChange, geometry.Heads), image.GetAddress(zoneChange));
        Assert.Equal(MacintoshGcrGeometry.Address(geometry.BlockCount - 1, geometry.Heads), image.GetAddress(geometry.BlockCount - 1));
        Assert.Throws<InvalidDataException>(() => MacintoshGcrSectorImageBuilder.Create(new byte[geometry.Capacity - 1], "mac", geometry));
    }

    /// <summary>Vérifie Apple II 13/16 secteurs, la priorité d'intégrité et la conservation des absences.</summary>
    [Fact]
    public void AppleIIBuilderSelectsGeometryIntegrityAndMissingBlocks()
    {
        var thirteen = Enumerable.Range(0, AppleIIGeometry.Dos32SectorsPerTrack).Select(number => Sector(number, AppleIIGeometry.SectorSize, true, (byte)number)).ToArray();
        var dos32 = AppleIISectorImageBuilder.Create([(0, thirteen)]);
        Assert.Equal(AppleIIGeometry.Dos32SectorsPerTrack, dos32.SectorsPerTrack);

        var bad = Sector(0, AppleIIGeometry.SectorSize, false, 1);
        var good = Sector(0, AppleIIGeometry.SectorSize, true, 2);
        var sixteen = Enumerable.Range(1, AppleIIGeometry.SectorsPerTrack - 1).Select(number => Sector(number, AppleIIGeometry.SectorSize, true, (byte)number)).Prepend(bad).Append(good).ToArray();
        var dos33 = AppleIISectorImageBuilder.Create([(0, sixteen)]);
        Assert.Equal(2, dos33.AvailableBlocks.Single(block => block.Address.Number == 0).Data[0]);

        var dense = AppleIISectorImageBuilder.ToDense([new SectorBlock(1, new(0, 0, 1), [1, 2])], 3, 2);
        Assert.Equal([0, 2], dense.MissingBlocks);
        Assert.Equal([0, 0, 1, 2, 0, 0], dense.Data);
        Assert.Throws<InvalidDataException>(() => AppleIISectorImageBuilder.ToDense([new SectorBlock(3, new(0, 0, 3), [1, 2])], 3, 2));
    }

    /// <summary>Vérifie RWTS18 avec doublons, absence de secteur valide et piste supérieure à 35.</summary>
    [Fact]
    public void Rwts18BuilderHandlesDuplicatesEmptyInputAndHighTracks()
    {
        var bad = Sector(0, AppleRwts18Format.SectorByteCount, false, 1);
        var good = Sector(0, AppleRwts18Format.SectorByteCount, true, 2);
        var image = AppleRwts18SectorImageBuilder.Create([(40, new[] { bad, good })]);
        Assert.Equal(41, image.Cylinders);
        Assert.Equal(2, image.AvailableBlocks.Single().Data[0]);
        var exception = Assert.Throws<InvalidDataException>(() => AppleRwts18SectorImageBuilder.Create([(0, Array.Empty<DecodedSector>())]));
        Assert.Contains("1 tracks", exception.Message, StringComparison.Ordinal);
        Assert.Contains("0 decoded sectors", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Crée un secteur décodé dont tous les octets portent la même valeur.</summary>
    private static DecodedSector Sector(int number, int size, bool integrity, byte value) => new(0, 0, number, 0, size, integrity, 0, Data: Enumerable.Repeat(value, size).ToArray());
}

/// <summary>Fournit une lecture concise de l'adresse d'un bloc disponible.</summary>
internal static class SectorImageBuilderTestExtensions
{
    /// <summary>Retourne l'adresse du bloc logique attendu par le test.</summary>
    public static SectorAddress GetAddress(this SectorImage image, int logicalBlock) => image.AvailableBlocks.Single(block => block.LogicalBlock == logicalBlock).Address;
}
