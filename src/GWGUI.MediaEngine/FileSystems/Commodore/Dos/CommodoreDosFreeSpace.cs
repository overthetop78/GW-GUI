namespace GWGUI.MediaEngine.FileSystems.Commodore.Dos;

/// <summary>Contient le nombre de blocs libres lorsqu'il a pu être lu complètement.</summary>
internal sealed record CommodoreDosFreeSpace
{
    /// <summary>Crée un résultat présent ou indéterminé.</summary>
    public CommodoreDosFreeSpace(int? freeBlocks) => FreeBlocks = freeBlocks;
    /// <summary>Nombre de blocs libres, ou aucune valeur si un BAM requis est illisible.</summary>
    public int? FreeBlocks { get; }
}
