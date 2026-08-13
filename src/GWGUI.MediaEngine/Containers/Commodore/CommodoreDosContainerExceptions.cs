using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Commodore;

/// <summary>Construit les erreurs communes aux Writers D64 et D71.</summary>
internal static class CommodoreDosContainerExceptions
{
    /// <summary>Signale une géométrie qui ne correspond à aucun conteneur pris en charge.</summary>
    public static InvalidDataException UnsupportedGeometry(SectorImage image) => new($"Commodore DOS image '{image.FormatId}' with geometry {image.Cylinders} tracks, {image.Heads} sides and {image.BlockCount} blocks cannot be written as D64 or D71.");
    /// <summary>Signale un bloc requis absent.</summary>
    public static InvalidDataException MissingBlock(int logicalBlock) => new($"Commodore DOS logical block {logicalBlock} is missing.");
    /// <summary>Signale un bloc dont la taille est invalide.</summary>
    public static InvalidDataException InvalidBlockSize(int logicalBlock, int actual, int expected) => new($"Commodore DOS logical block {logicalBlock} contains {actual} bytes; expected {expected}.");
    /// <summary>Signale un diagnostic absent alors que la carte d'erreurs doit être conservée.</summary>
    public static InvalidDataException MissingDiagnostic(int logicalBlock) => new($"Commodore DOS logical block {logicalBlock} has no diagnostic code to preserve in the error map.");
}
