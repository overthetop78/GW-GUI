namespace GWGUI.MediaEngine.Containers.Raw;

/// <summary>Construit les erreurs du Writer d'images sectorielles linéaires.</summary>
public static class LinearSectorImageWriterExceptions
{
    /// <summary>Signale une géométrie incompatible avec le conteneur cible.</summary>
    public static InvalidDataException InvalidGeometry(string formatId, int blockSize, int cylinders, int heads, int sectorsPerTrack, string expectedFormatId, int expectedBlockSize, int expectedCylinders, int expectedHeads, int expectedSectorsPerTrack) => new($"Sector image '{formatId}' uses {cylinders}x{heads}x{sectorsPerTrack} blocks of {blockSize} bytes; expected '{expectedFormatId}' with {expectedCylinders}x{expectedHeads}x{expectedSectorsPerTrack} blocks of {expectedBlockSize} bytes.");

    /// <summary>Signale un bloc logique absent.</summary>
    public static InvalidDataException MissingBlock(int logicalBlock) => new($"Logical block {logicalBlock} is missing and cannot be written to a complete linear sector image.");

    /// <summary>Signale une taille de bloc incohérente.</summary>
    public static InvalidDataException InvalidBlockSize(int logicalBlock, int actual, int expected) => new($"Logical block {logicalBlock} contains {actual} bytes; expected {expected} bytes.");
}
