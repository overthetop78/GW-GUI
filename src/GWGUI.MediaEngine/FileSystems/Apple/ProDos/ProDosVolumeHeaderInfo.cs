namespace GWGUI.MediaEngine.FileSystems.Apple.ProDos;

/// <summary>Contient les champs validés de l'en-tête de volume ProDOS.</summary>
internal sealed record ProDosVolumeHeaderInfo
{
    /// <summary>Crée un en-tête de volume décodé.</summary>
    public ProDosVolumeHeaderInfo(string name, int bitmapBlock, int totalBlocks, DateTimeOffset? created, IReadOnlyList<byte> rootBlock)
    {
        Name = name;
        BitmapBlock = bitmapBlock;
        TotalBlocks = totalBlocks;
        Created = created;
        RootBlock = rootBlock;
    }

    /// <summary>Nom du volume.</summary>
    public string Name { get; }
    /// <summary>Premier bloc du bitmap.</summary>
    public int BitmapBlock { get; }
    /// <summary>Nombre total de blocs annoncé.</summary>
    public int TotalBlocks { get; }
    /// <summary>Date de création du volume.</summary>
    public DateTimeOffset? Created { get; }
    /// <summary>Copie du bloc racine validé.</summary>
    public IReadOnlyList<byte> RootBlock { get; }
}
