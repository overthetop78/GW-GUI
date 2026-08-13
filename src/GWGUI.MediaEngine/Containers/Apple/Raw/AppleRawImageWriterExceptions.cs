namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Construit les erreurs de validation des images sectorielles Apple brutes.</summary>
internal static class AppleRawImageWriterExceptions
{
    /// <summary>Signale une combinaison de format et d'extension non prise en charge.</summary>
    public static NotSupportedException UnsupportedTarget(string formatId, string extension) => new($"Apple raw target '{formatId}' with extension '{extension}' is not supported.");

    /// <summary>Signale une géométrie incompatible avec le format cible.</summary>
    public static InvalidDataException InvalidGeometry(string formatId, int blockSize, int blockCount, long capacity, int expectedBlockSize, int expectedBlockCount, int expectedCapacity) => new($"Apple image '{formatId}' has {blockCount} blocks of {blockSize} bytes and capacity {capacity}; expected {expectedBlockCount} blocks of {expectedBlockSize} bytes and capacity {expectedCapacity}.");

    /// <summary>Signale un bloc absent de l'image source.</summary>
    public static InvalidDataException MissingBlock(int logicalBlock) => new($"Apple image is missing logical block {logicalBlock}.");

    /// <summary>Signale un bloc dont la taille ne correspond pas au format cible.</summary>
    public static InvalidDataException InvalidBlockSize(int logicalBlock, int actual, int expected) => new($"Apple logical block {logicalBlock} contains {actual} bytes; expected {expected}.");
}
