using GWGUI.MediaEngine.Containers.Atari.Msa;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture déterministe des pistes brutes et compressées d'un conteneur MSA.</summary>
public sealed class MsaReaderTests
{
    /// <summary>Vérifie la géométrie, l'ordre, le contenu et l'identifiant reconstruits depuis l'image locale.</summary>
    [Fact]
    public async Task ReadsRawAndCompressedTracksFromKnownImage()
    {
        var image = await new MsaReader().ReadAsync(KnownImagePath());

        Assert.Equal(DiskImageFormatIds.AtariStFromCapacity(1024), image.FormatId);
        Assert.Equal(1, image.Cylinders);
        Assert.Equal(2, image.Heads);
        Assert.Equal(1, image.SectorsPerTrack);
        Assert.Equal(512, image.BlockSize);
        Assert.Equal(2, image.BlockCount);
        Assert.Equal(Enumerable.Range(0, 512).Select(index => (byte)(index % 256)), image.GetBlock(0).ToArray());
        Assert.Equal(Enumerable.Repeat((byte)0xA5, 512), image.GetBlock(1).ToArray());
        Assert.Equal((0, 0, 1), AddressOf(image.AvailableBlocks.Single(block => block.LogicalBlock == 0)));
        Assert.Equal((0, 1, 1), AddressOf(image.AvailableBlocks.Single(block => block.LogicalBlock == 1)));
    }

    /// <summary>Vérifie le rejet des altérations de signature, géométrie, longueur de piste et séquence RLE.</summary>
    [Fact]
    public async Task RejectsInvalidContainerStructures()
    {
        var source = await File.ReadAllBytesAsync(KnownImagePath());

        await AssertRejectedAsync(source, bytes => bytes[0] = 0);
        await AssertRejectedAsync(source, bytes => { bytes[2] = 0; bytes[3] = 0; });
        await AssertRejectedAsync(source[..(12 + 511)], _ => { });
        await AssertRejectedAsync(source, bytes => { bytes[^2] = 0x02; bytes[^1] = 0x01; });
    }

    /// <summary>Retourne l'adresse CHS d'un bloc sous une forme directement comparable.</summary>
    private static (int Cylinder, int Head, int Number) AddressOf(GWGUI.MediaEngine.SectorImages.SectorBlock block) => (block.Address.Cylinder, block.Address.Head, block.Address.Number);

    /// <summary>Écrit une variante temporaire, vérifie son rejet puis supprime le fichier.</summary>
    private static async Task AssertRejectedAsync(byte[] source, Action<byte[]> mutate)
    {
        var bytes = source.ToArray();
        mutate(bytes);
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.msa");
        try
        {
            await File.WriteAllBytesAsync(path, bytes);
            await Assert.ThrowsAsync<InvalidDataException>(() => new MsaReader().ReadAsync(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Retourne le chemin de l'image MSA locale obligatoire.</summary>
    private static string KnownImagePath()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "_generated", "Atari", "msa-raw-and-rle.msa");
        return File.Exists(path) ? path : throw new FileNotFoundException("L'image MSA de test est introuvable.", path);
    }

    /// <summary>Localise la racine du dépôt courant.</summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("La racine du dépôt est introuvable.");
    }
}
