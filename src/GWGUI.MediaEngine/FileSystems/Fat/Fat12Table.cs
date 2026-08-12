using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.FileSystems.Fat;

/// <summary>Décode les entrées de douze bits d'une table FAT12.</summary>
public static class Fat12Table
{
    /// <summary>Valeur d'un cluster libre.</summary>
    public const int FreeCluster = 0x000;
    /// <summary>Première valeur réservée.</summary>
    public const int FirstReservedCluster = 0xff0;
    /// <summary>Dernière valeur réservée.</summary>
    public const int LastReservedCluster = 0xff6;
    /// <summary>Valeur d'un cluster défectueux.</summary>
    public const int BadCluster = 0xff7;
    /// <summary>Première valeur de fin de chaîne.</summary>
    public const int FirstEndOfChain = 0xff8;
    /// <summary>Dernière valeur de fin de chaîne.</summary>
    public const int LastEndOfChain = 0xfff;

    /// <summary>Tente de décoder une entrée paire ou impaire.</summary>
    /// <param name="fat">Octets de la FAT.</param>
    /// <param name="cluster">Numéro de cluster.</param>
    /// <param name="value">Valeur décodée.</param>
    /// <returns><see langword="true"/> lorsque les deux octets nécessaires sont disponibles.</returns>
    public static bool TryRead(ReadOnlySpan<byte> fat, int cluster, out int value)
    {
        var offset = cluster + cluster / 2;
        if (offset + 1 >= fat.Length) { value = 0; return false; }
        var pair = fat[offset] | fat[offset + 1] << BitPrimitives.BitsPerByte;
        value = (cluster & 1) == 0 ? pair & LastEndOfChain : pair >> 4;
        return true;
    }
}
