using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Coherent;

/// <summary>Écrit un dump sectoriel COHERENT dans l'ordre zoné du Commodore 900.</summary>
public sealed class CoherentRawImageWriter
{
    /// <summary>Valide chaque bloc et écrit atomiquement le dump BIN ou IMG.</summary>
    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        ValidateImage(image);
        var bytes = new byte[checked(image.BlockCount * Commodore900Geometry.SectorSize)];
        for (var logicalBlock = 0; logicalBlock < image.BlockCount; logicalBlock++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!image.TryGetBlock(logicalBlock, out var block)) throw new InvalidDataException($"Le bloc Commodore 900 {logicalBlock} est absent.");
            if (block.Address != Commodore900Geometry.AddressOf(logicalBlock)) throw new InvalidDataException($"Le bloc Commodore 900 {logicalBlock} ne correspond pas à l'ordre physique documenté.");
            if (block.Data.Count != Commodore900Geometry.SectorSize) throw new InvalidDataException($"Le bloc Commodore 900 {logicalBlock} ne contient pas {Commodore900Geometry.SectorSize} octets.");
            block.Data.ToArray().CopyTo(bytes, logicalBlock * Commodore900Geometry.SectorSize);
        }
        await WriteAtomicallyAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Contrôle l'identifiant, la géométrie et la longueur logique de l'image.</summary>
    private static void ValidateImage(SectorImage image)
    {
        if (!image.FormatId.Equals(DiskImageFormatIds.Commodore900Coherent, StringComparison.OrdinalIgnoreCase) || image.BlockSize != Commodore900Geometry.SectorSize || image.Cylinders != Commodore900Geometry.CylinderCount || image.Heads != Commodore900Geometry.HeadCount || image.SectorsPerTrack != Commodore900Geometry.MaximumSectorsPerTrack || image.BlockCount > Commodore900Geometry.BlockCount) throw new InvalidDataException("L'image sectorielle n'utilise pas la géométrie zonée du Commodore 900.");
    }

    /// <summary>Écrit dans un fichier temporaire voisin avant de remplacer la cible.</summary>
    private static async Task WriteAtomicallyAsync(string path, byte[] bytes, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes, cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
