namespace GWGUI.MediaEngine.SectorImages.Builders;

/// <summary>Indique la base utilisée par les numéros de secteurs physiques.</summary>
public enum SectorNumbering
{
    /// <summary>Le premier secteur porte le numéro zéro.</summary>
    ZeroBased,
    /// <summary>Le premier secteur porte le numéro un.</summary>
    OneBased
}

/// <summary>Décrit une géométrie linéaire validée avant la construction de ses blocs.</summary>
public sealed class LinearSectorImageGeometry
{
    /// <summary>Crée une géométrie dont toutes les dimensions sont strictement positives.</summary>
    public LinearSectorImageGeometry(int blockSize, int cylinders, int heads, int sectorsPerTrack, SectorNumbering numbering = SectorNumbering.ZeroBased)
    {
        if (blockSize <= 0 || cylinders <= 0 || heads <= 0 || sectorsPerTrack <= 0) throw new ArgumentOutOfRangeException(nameof(blockSize), "Linear geometry dimensions must be positive.");
        BlockSize = blockSize;
        Cylinders = cylinders;
        Heads = heads;
        SectorsPerTrack = sectorsPerTrack;
        Numbering = numbering;
        BlockCount = checked(cylinders * heads * sectorsPerTrack);
        Capacity = checked(BlockCount * blockSize);
    }

    /// <summary>Taille d'un bloc en octets.</summary>
    public int BlockSize { get; }
    /// <summary>Nombre de cylindres.</summary>
    public int Cylinders { get; }
    /// <summary>Nombre de faces.</summary>
    public int Heads { get; }
    /// <summary>Nombre de secteurs par piste.</summary>
    public int SectorsPerTrack { get; }
    /// <summary>Base de numérotation des secteurs.</summary>
    public SectorNumbering Numbering { get; }
    /// <summary>Nombre total de blocs.</summary>
    public int BlockCount { get; }
    /// <summary>Capacité exacte en octets.</summary>
    public int Capacity { get; }
}
