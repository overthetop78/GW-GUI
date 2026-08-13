using GWGUI.MediaEngine.Conversion.Apple;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Écrit les images Apple sectorielles brutes en ordre DOS ou ProDOS explicite.</summary>
public sealed class AppleRawImageWriter
{
    /// <summary>Valide la cible, construit sa charge utile puis remplace atomiquement le fichier de destination.</summary>
    public async Task WriteAsync(SectorImage image, string path, string targetFormatId, CancellationToken cancellationToken = default)
    {
        var payload = BuildPayload(image, targetFormatId, Path.GetExtension(path));
        await WriteAtomicallyAsync(path, payload, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Construit la charge utile sectorielle correspondant au format et au conteneur demandés.</summary>
    internal static byte[] BuildPayload(SectorImage image, string targetFormatId, string extension)
    {
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.D13, StringComparison.OrdinalIgnoreCase))
            return BuildLinear(image, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount * AppleIIGeometry.Dos32SectorsPerTrack, AppleIIGeometry.Dos32Capacity);
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase) && (extension.Equals(DiskImageFileExtensions.Do, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)))
            return BuildLinear(image, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount * AppleIIGeometry.SectorsPerTrack, AppleIIGeometry.Capacity);
        if (IsProDos140(targetFormatId) && extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase))
            return BuildLinear(image, AppleIIGeometry.ProDosBlockSize, AppleIIGeometry.TrackCount * AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.Capacity);
        if (IsProDos140(targetFormatId) && (extension.Equals(DiskImageFileExtensions.Do, StringComparison.OrdinalIgnoreCase) || extension.Equals(DiskImageFileExtensions.Dsk, StringComparison.OrdinalIgnoreCase)))
            return AppleIISectorOrderConverter.ProDosToDos(BuildLinear(image, AppleIIGeometry.ProDosBlockSize, AppleIIGeometry.TrackCount * AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.Capacity));
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase) && extension.Equals(DiskImageFileExtensions.Po, StringComparison.OrdinalIgnoreCase))
            return BuildLinear(image, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.SingleSidedBlockCount * MacintoshGcrGeometry.DoubleSidedHeadCount, MacintoshGcrGeometry.Capacity800K);
        throw AppleRawImageWriterExceptions.UnsupportedTarget(targetFormatId, extension);
    }

    /// <summary>Construit une charge utile 2IMG dans l'ordre imposé par son type d'image.</summary>
    internal static byte[] BuildTwoImgPayload(SectorImage image, string targetFormatId)
    {
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos113, StringComparison.OrdinalIgnoreCase)) return BuildLinear(image, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount * AppleIIGeometry.Dos32SectorsPerTrack, AppleIIGeometry.Dos32Capacity);
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIAppleDos140, StringComparison.OrdinalIgnoreCase)) return BuildLinear(image, AppleIIGeometry.SectorSize, AppleIIGeometry.TrackCount * AppleIIGeometry.SectorsPerTrack, AppleIIGeometry.Capacity);
        if (IsProDos140(targetFormatId)) return BuildLinear(image, AppleIIGeometry.ProDosBlockSize, AppleIIGeometry.TrackCount * AppleIIGeometry.ProDosBlocksPerTrack, AppleIIGeometry.Capacity);
        if (targetFormatId.Equals(DiskImageFormatIds.AppleIIProDos800, StringComparison.OrdinalIgnoreCase)) return BuildLinear(image, MacintoshGcrGeometry.BlockSize, MacintoshGcrGeometry.SingleSidedBlockCount * MacintoshGcrGeometry.DoubleSidedHeadCount, MacintoshGcrGeometry.Capacity800K);
        throw AppleRawImageWriterExceptions.UnsupportedTarget(targetFormatId, DiskImageFileExtensions.TwoMg);
    }

    /// <summary>Indique si la cible utilise 280 blocs ProDOS de 512 octets.</summary>
    internal static bool IsProDos140(string formatId) => formatId.Equals(DiskImageFormatIds.AppleIIProDos140, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.AppleIIISos, StringComparison.OrdinalIgnoreCase);

    /// <summary>Concatène tous les blocs après validation stricte de leur géométrie et de leur taille.</summary>
    private static byte[] BuildLinear(SectorImage image, int blockSize, int blockCount, int capacity)
    {
        if (image.BlockSize != blockSize || image.BlockCount != blockCount || image.Capacity != capacity) throw AppleRawImageWriterExceptions.InvalidGeometry(image.FormatId, image.BlockSize, image.BlockCount, image.Capacity, blockSize, blockCount, capacity);
        var output = new byte[capacity];
        for (var logicalBlock = 0; logicalBlock < blockCount; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block)) throw AppleRawImageWriterExceptions.MissingBlock(logicalBlock);
            if (block.Data.Count != blockSize) throw AppleRawImageWriterExceptions.InvalidBlockSize(logicalBlock, block.Data.Count, blockSize);
            block.Data.ToArray().CopyTo(output, logicalBlock * blockSize);
        }
        return output;
    }

    /// <summary>Écrit dans un fichier temporaire local puis remplace la destination terminée.</summary>
    internal static async Task WriteAtomicallyAsync(string path, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes.ToArray(), cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }
}
