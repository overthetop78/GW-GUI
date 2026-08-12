namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Construit les erreurs de validation des cartes FileCore.</summary>
public static class AcornFileCoreExceptions
{
    /// <summary>Crée une erreur de plage de bits.</summary>
    public static ArgumentOutOfRangeException InvalidBitRange(int offset, int length, int capacity) => new(nameof(offset), offset, $"Bit range {offset}..{offset + length} exceeds capacity {capacity}.");
    /// <summary>Crée une erreur de limites de zone.</summary>
    public static ArgumentOutOfRangeException InvalidZone(int start, int end, int capacity) => new(nameof(start), start, $"Zone range {start}..{end} exceeds capacity {capacity}.");
    /// <summary>Crée une erreur de décalage.</summary>
    public static ArgumentOutOfRangeException InvalidShift(int shift) => new(nameof(shift), shift, "The FileCore shift must fit an Int32 bit width.");
}
