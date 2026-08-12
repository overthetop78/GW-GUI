using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.TeleDisk;

/// <summary>Classe une image sectorielle reconstruite depuis un conteneur TeleDisk.</summary>
internal static class Td0SectorImageClassifier
{
    /// <summary>Détermine l'identifiant de format depuis le secteur de démarrage et la géométrie observée.</summary>
    /// <param name="blocks">Blocs sectoriels reconstruits.</param>
    /// <param name="blockSize">Taille sectorielle dominante, en octets.</param>
    /// <param name="cylinders">Nombre de cylindres.</param>
    /// <param name="heads">Nombre de faces.</param>
    /// <param name="sectorsPerTrack">Nombre maximal de secteurs par piste.</param>
    /// <returns>Identifiant IBM reconnu ou identifiant UCSD de repli.</returns>
    public static string Detect(IReadOnlyList<SectorBlock> blocks, int blockSize, int cylinders, int heads, int sectorsPerTrack)
    {
        var boot = blocks.FirstOrDefault(block => block.Address.Cylinder == 0 && block.Address.Head == 0 && block.Address.Number == 1)?.Data;
        var imageLength = checked(cylinders * heads * sectorsPerTrack * blockSize);
        var hasFatBpb = boot is not null && FatBpbGeometryDetector.TryDetect(boot.ToArray(), imageLength, out _);
        var hasDosBootJump = boot is { Count: > 0 } && boot[0] is FatBootSectorLayout.ShortJumpOpcode or FatBootSectorLayout.NearJumpOpcode;
        if ((hasFatBpb || hasDosBootJump) && blockSize == FatBootSectorLayout.SectorSize)
        {
            return IbmPcGeometryCatalog.TryFromCapacity(imageLength, out var geometry) && geometry.Cylinders == cylinders && geometry.Heads == heads && geometry.SectorsPerTrack == sectorsPerTrack ? geometry.FormatId : DiskImageFormatIds.IbmScan;
        }
        return DiskImageFormatIds.UcsdIbmMfm;
    }
}
