using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Primitives;
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
        var hasFatBpb = boot is { Count: >= 36 } && (boot[11] | boot[12] << BitPrimitives.BitsPerByte) == blockSize && boot[13] > 0
            && (boot[24] | boot[25] << BitPrimitives.BitsPerByte) is > 0 and <= 64 && (boot[26] | boot[27] << BitPrimitives.BitsPerByte) is > 0 and <= 8;
        var hasDosBootJump = boot is { Count: >= 3 } && boot[0] is 0xeb or 0xe9;
        if ((hasFatBpb || hasDosBootJump) && blockSize == 512)
        {
            return (cylinders, heads, sectorsPerTrack) switch
            {
                (40, 1, 8) => DiskImageFormatIds.Ibm160,
                (40, 1, 9) => DiskImageFormatIds.Ibm180,
                (DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 8) => DiskImageFormatIds.Ibm320,
                (DiskGeometryConstants.FortyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9) => DiskImageFormatIds.Ibm360,
                (DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 9) => DiskImageFormatIds.Ibm720,
                (DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 15) => DiskImageFormatIds.Ibm1200,
                (DiskGeometryConstants.EightyTrackCylinderCount, DiskGeometryConstants.DoubleSidedHeadCount, 18) => DiskImageFormatIds.Ibm1440,
                _ => DiskImageFormatIds.IbmScan
            };
        }
        return DiskImageFormatIds.UcsdIbmMfm;
    }
}
