using System.IO;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture déterministe des conteneurs TeleDisk non compressés.</summary>
public sealed class Td0ReaderTests
{
    private const int CommentHeaderOffset = 12;
    private const int TrackHeaderOffset = 36;
    private const int FirstSectorHeaderOffset = 40;
    private const int FirstSectorEncodingOffset = 48;
    private const int FirstSectorPayloadOffset = 49;

    /// <summary>Vérifie le commentaire, les trois encodages, l'ordre, l'intégrité et les CRC de l'image connue.</summary>
    [Fact]
    public async Task ReadsKnownRawRepeatedAndRleSectors()
    {
        var image = await new Td0Reader().ReadAsync(KnownImagePath());

        Assert.Equal(DiskImageFormatIds.UcsdIbmMfm, image.FormatId);
        Assert.Equal(1, image.Cylinders);
        Assert.Equal(1, image.Heads);
        Assert.Equal(3, image.SectorsPerTrack);
        Assert.Equal(Enumerable.Range(0, 128).Select(index => (byte)index), image.GetBlock(0).ToArray());
        Assert.Equal(Enumerable.Range(0, 64).SelectMany(_ => new byte[] { 0xAA, 0x55 }), image.GetBlock(1).ToArray());
        Assert.Equal(Enumerable.Range(0, 64).SelectMany(_ => new byte[] { 0x12, 0x34 }), image.GetBlock(2).ToArray());
        Assert.True(image.AvailableBlocks.Single(block => block.LogicalBlock == 0).IntegrityValid);
        Assert.True(image.AvailableBlocks.Single(block => block.LogicalBlock == 1).IntegrityValid);
        Assert.False(image.AvailableBlocks.Single(block => block.LogicalBlock == 2).IntegrityValid);
    }

    /// <summary>Vérifie le diagnostic propre à la signature de compression avancée.</summary>
    [Fact]
    public async Task RejectsAdvancedCompressionSignatureExplicitly()
    {
        var exception = await AssertRejectedAsync(bytes => { bytes[0] = (byte)'t'; bytes[1] = (byte)'d'; });
        Assert.Contains("Advanced TeleDisk compression", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Vérifie le rejet des sections tronquées, encodages invalides et CRC altérés.</summary>
    [Fact]
    public async Task RejectsTruncatedInvalidAndCorruptedStructures()
    {
        await AssertRejectedAsync(bytes => bytes[..(CommentHeaderOffset + 5)]);
        await AssertRejectedAsync(bytes => bytes[..(TrackHeaderOffset + 2)]);
        await AssertRejectedAsync(bytes => bytes[..(FirstSectorHeaderOffset + 3)]);
        await AssertRejectedAsync(bytes => bytes[..(FirstSectorPayloadOffset + 127)]);
        await AssertRejectedAsync(bytes => { bytes[FirstSectorEncodingOffset] = 9; return bytes; });
        await AssertRejectedAsync(bytes => { bytes[10] ^= 1; return bytes; });
        await AssertRejectedAsync(bytes => { bytes[TrackHeaderOffset + 3] ^= 1; return bytes; });
        await AssertRejectedAsync(bytes => { bytes[FirstSectorPayloadOffset] ^= 1; return bytes; });
    }

    /// <summary>Crée une variante temporaire et retourne l'erreur produite par le lecteur public.</summary>
    private static Task<InvalidDataException> AssertRejectedAsync(Action<byte[]> mutate) => AssertRejectedAsync(bytes => { mutate(bytes); return bytes; });

    /// <summary>Crée une variante temporaire et retourne l'erreur produite par le lecteur public.</summary>
    private static async Task<InvalidDataException> AssertRejectedAsync(Func<byte[], byte[]> mutate)
    {
        var bytes = mutate(await File.ReadAllBytesAsync(KnownImagePath()));
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.td0");
        try
        {
            await File.WriteAllBytesAsync(path, bytes);
            return await Assert.ThrowsAsync<InvalidDataException>(() => new Td0Reader().ReadAsync(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Retourne le chemin de l'image TeleDisk locale obligatoire.</summary>
    private static string KnownImagePath()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "_generated", "TeleDisk", "raw-repeated-rle.td0");
        return File.Exists(path) ? path : throw new FileNotFoundException("L'image TeleDisk de test est introuvable.", path);
    }

    /// <summary>Localise la racine du dépôt courant.</summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("La racine du dépôt est introuvable.");
    }
}
