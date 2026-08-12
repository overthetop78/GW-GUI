namespace GWGUI.MediaEngine.Geometries.Apple;

/// <summary>Construit les erreurs de validation des géométries Apple.</summary>
internal static class AppleGeometryExceptions
{
    /// <summary>Crée l'erreur signalant un cylindre extérieur à la géométrie.</summary>
    public static ArgumentOutOfRangeException InvalidCylinder(int cylinder, int cylinderCount) => new(nameof(cylinder), cylinder, $"Cylinder must be between 0 and {cylinderCount - 1}.");
    /// <summary>Crée l'erreur signalant un nombre de faces incompatible avec la géométrie Macintosh GCR.</summary>
    public static ArgumentOutOfRangeException InvalidHeadCount(int heads) => new(nameof(heads), heads, $"Apple Macintosh geometry supports {MacintoshGcrGeometry.SingleSidedHeadCount} or {MacintoshGcrGeometry.DoubleSidedHeadCount} heads.");
    /// <summary>Crée l'erreur signalant un bloc logique extérieur à la capacité de l'image.</summary>
    public static InvalidDataException InvalidLogicalBlock(int logicalBlock, int blockCount) => new($"Apple logical block {logicalBlock} is outside capacity of {blockCount} blocks.");
    /// <summary>Crée l'erreur signalant un bloc logique extérieur à une géométrie Macintosh définie par son nombre de faces.</summary>
    public static InvalidDataException InvalidMacintoshLogicalBlock(int logicalBlock, int heads, int blockCount) => new($"Apple Macintosh logical block {logicalBlock} is outside capacity of {blockCount} blocks for {heads} heads.");
    /// <summary>Crée l'erreur signalant une capacité d'image Apple non prise en charge.</summary>
    public static InvalidDataException UnsupportedCapacity(int capacity) => new($"Apple image capacity {capacity} bytes is not supported.");
}
