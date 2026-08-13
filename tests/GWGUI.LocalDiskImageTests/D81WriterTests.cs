using System.IO;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.Tests;

/// <summary>Vérifie l'écriture D81, sa géométrie physique et la conservation de Commodore DOS.</summary>
public sealed class D81WriterTests
{
    /// <summary>Vérifie que les 80×2×10 secteurs physiques couvrent exactement les 3 200 blocs logiques.</summary>
    [Fact]
    public void PhysicalGeometryCoversEveryLogicalBlockExactlyOnce()
    {
        var blocks = new HashSet<int>();
        for (var cylinder = 0; cylinder < Commodore1581Geometry.LogicalCylinderCount; cylinder++)
            for (var head = 0; head < Commodore1581Geometry.PhysicalHeadCount; head++)
                for (var sector = 1; sector <= Commodore1581Geometry.PhysicalSectorsPerTrack; sector++)
                {
                    var first = Commodore1581Geometry.PhysicalSectorToLogicalBlock(cylinder, head, sector);
                    Assert.True(blocks.Add(first));
                    Assert.True(blocks.Add(first + 1));
                }
        Assert.Equal(D81Layout.LogicalBlockCount, blocks.Count);
        Assert.Equal(Enumerable.Range(0, D81Layout.LogicalBlockCount), blocks.Order());
    }

    /// <summary>Vérifie que la conversion D81 conserve exactement les octets, le BAM, le répertoire et le fichier.</summary>
    [Fact]
    public async Task ConversionRoundTripPreservesBamDirectoryAndFile()
    {
        var source = TemporaryPath();
        var output = TemporaryPath();
        var bytes = CreateFileSystemImage();
        try
        {
            await File.WriteAllBytesAsync(source, bytes);
            await MediaEngineFactory.CreateD81ConversionService().ConvertAsync(source, output, DiskImageFormatIds.Commodore1581);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(output));
            var image = await new D81Reader().ReadAsync(output);
            var volume = new CommodoreDosFileSystemReader().Read(image);
            Assert.Equal("TEST D81", volume.Name);
            Assert.Equal(30 * Commodore1581Geometry.LogicalBlockSize, volume.FreeBytes);
            var entry = Assert.Single(volume.Entries);
            Assert.Equal("HELLO", entry.Name);
            Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, entry.Content);
            Assert.Empty(volume.Warnings);
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    /// <summary>Vérifie que le routage interne accepte uniquement la combinaison Commodore 1581 et D81.</summary>
    [Fact]
    public void RoutesOnlyCommodore1581D81()
    {
        Assert.True(GWGUI.MediaEngine.Conversion.Commodore.D81ConversionService.CanCreate(DiskImageFormatIds.Commodore1581, DiskImageFileExtensions.D81));
        Assert.False(GWGUI.MediaEngine.Conversion.Commodore.D81ConversionService.CanCreate(DiskImageFormatIds.Commodore1571, DiskImageFileExtensions.D81));
        Assert.False(GWGUI.MediaEngine.Conversion.Commodore.D81ConversionService.CanCreate(DiskImageFormatIds.Commodore1581, DiskImageFileExtensions.D64));
    }

    /// <summary>Construit un petit volume D81 cohérent avec un fichier de cinq octets.</summary>
    private static byte[] CreateFileSystemImage()
    {
        var image = new byte[D81Layout.ImageLength];
        var header = Sector(image, Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.HeaderSector);
        header[0] = Commodore1581DosLayout.HeaderTrack;
        header[1] = Commodore1581DosLayout.DirectorySector;
        header[2] = Commodore1581DosLayout.HeaderSignature;
        WritePetscii(header, Commodore1581DosLayout.VolumeNameOffset, "TEST D81");
        foreach (var bamSector in new[] { Commodore1581DosLayout.FirstBamSector, Commodore1581DosLayout.SecondBamSector })
        {
            var bam = Sector(image, Commodore1581DosLayout.HeaderTrack, bamSector);
            for (var entry = 0; entry < Commodore1581DosLayout.BamEntryCount; entry++) bam[Commodore1581DosLayout.BamEntriesOffset + entry * Commodore1581DosLayout.BamEntrySize] = entry == 0 ? (byte)15 : (byte)0;
        }
        var directory = Sector(image, Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.DirectorySector);
        directory[2] = (byte)(CommodoreDosFileType.Closed | CommodoreDosFileType.Prg);
        directory[3] = 1;
        directory[4] = 0;
        WritePetscii(directory, 5, "HELLO");
        directory[30] = 1;
        var file = Sector(image, 1, 0);
        file[0] = 0;
        file[1] = 6;
        new byte[] { 1, 2, 3, 4, 5 }.AsSpan().CopyTo(file[2..]);
        return image;
    }

    /// <summary>Retourne un secteur logique mutable d'une image D81.</summary>
    private static Span<byte> Sector(byte[] image, int track, int sector) => image.AsSpan(Commodore1581Geometry.ToLogicalBlock(track, sector) * Commodore1581Geometry.LogicalBlockSize, Commodore1581Geometry.LogicalBlockSize);

    /// <summary>Écrit un texte PETSCII ASCII et complète le champ avec des espaces insécables Commodore.</summary>
    private static void WritePetscii(Span<byte> destination, int offset, string value)
    {
        destination.Slice(offset, 16).Fill(0xA0);
        System.Text.Encoding.ASCII.GetBytes(value).CopyTo(destination[offset..]);
    }

    /// <summary>Crée un chemin D81 temporaire.</summary>
    private static string TemporaryPath() => Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.d81");
}
