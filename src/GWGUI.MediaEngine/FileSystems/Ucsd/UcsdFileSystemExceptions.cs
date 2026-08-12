namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Construit les diagnostics propres aux volumes UCSD.</summary>
public static class UcsdFileSystemExceptions
{
    /// <summary>Crée l'erreur signalant un répertoire absent.</summary>
    public static InvalidDataException MissingDirectory(int block, int observedSize) => new($"The UCSD directory at block {block} is unavailable or has size {observedSize}.");
    /// <summary>Crée l'erreur signalant un ordre des octets indéterminé.</summary>
    public static InvalidDataException UnknownByteOrder(ReadOnlySpan<byte> bytes) => new($"The UCSD byte order cannot be determined from {Convert.ToHexString(bytes[..Math.Min(4, bytes.Length)])}.");
    /// <summary>Construit l'avertissement signalant une plage incomplète.</summary>
    public static string IncompleteRange(int firstBlock, int blockCount, int obtainedLength) => $"UCSD block range {firstBlock}..{firstBlock + blockCount - 1} is incomplete ({obtainedLength} bytes obtained).";
    /// <summary>Construit l'avertissement signalant une entrée invalide.</summary>
    public static string InvalidEntry(int index, string name, int firstBlock, int lastBlock) => $"UCSD directory entry {index} ('{name}') has invalid block range {firstBlock}..{lastBlock}.";
}
