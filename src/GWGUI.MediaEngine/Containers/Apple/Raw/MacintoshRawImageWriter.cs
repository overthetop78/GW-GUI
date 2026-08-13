using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Écrit les images sectorielles brutes Macintosh GCR 400/800 Kio et MFM 1,44 Mio.</summary>
public sealed class MacintoshRawImageWriter
{
    /// <summary>Valide la géométrie et l'ordre zoné ou linéaire avant l'écriture atomique.</summary>
    public async Task WriteAsync(SectorImage image, string path, CancellationToken cancellationToken = default)
    {
        var bytes = BuildPayload(image);
        await AppleRawImageWriter.WriteAtomicallyAsync(path, bytes, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Construit les octets bruts dans l'ordre logique Macintosh.</summary>
    internal static byte[] BuildPayload(SectorImage image)
    {
        var expectedBlocks = image.Capacity switch
        {
            MacintoshGcrGeometry.Capacity400K => MacintoshGcrGeometry.SingleSidedBlockCount,
            MacintoshGcrGeometry.Capacity800K => MacintoshGcrGeometry.SingleSidedBlockCount * MacintoshGcrGeometry.DoubleSidedHeadCount,
            MacintoshMfmGeometry.Capacity => MacintoshMfmGeometry.SectorCount,
            _ => throw new InvalidDataException($"La capacité Macintosh {image.Capacity} n'est pas prise en charge.")
        };
        if (image.BlockSize != MacintoshGcrGeometry.BlockSize || image.BlockCount != expectedBlocks) throw new InvalidDataException("L'image ne possède pas une géométrie Macintosh complète.");
        var bytes = new byte[checked(expectedBlocks * MacintoshGcrGeometry.BlockSize)];
        for (var logicalBlock = 0; logicalBlock < expectedBlocks; logicalBlock++)
        {
            if (!image.TryGetBlock(logicalBlock, out var block)) throw new InvalidDataException($"Le bloc Macintosh {logicalBlock} est absent.");
            var expectedAddress = image.Capacity == MacintoshMfmGeometry.Capacity ? MfmAddress(logicalBlock) : MacintoshGcrGeometry.Address(logicalBlock, image.Heads);
            if (block.Address != expectedAddress) throw new InvalidDataException($"Le bloc Macintosh {logicalBlock} ne correspond pas à sa géométrie.");
            if (block.Data.Count != MacintoshGcrGeometry.BlockSize) throw new InvalidDataException($"Le bloc Macintosh {logicalBlock} n'a pas la taille attendue.");
            block.Data.ToArray().CopyTo(bytes, logicalBlock * MacintoshGcrGeometry.BlockSize);
        }
        return bytes;
    }

    /// <summary>Retourne l'adresse CHS uniforme d'un bloc Macintosh MFM.</summary>
    private static SectorAddress MfmAddress(int logicalBlock)
    {
        var blocksPerCylinder = MacintoshMfmGeometry.HeadCount * MacintoshMfmGeometry.SectorsPerTrack;
        return new(logicalBlock / blocksPerCylinder, logicalBlock / MacintoshMfmGeometry.SectorsPerTrack % MacintoshMfmGeometry.HeadCount, logicalBlock % MacintoshMfmGeometry.SectorsPerTrack);
    }
}
