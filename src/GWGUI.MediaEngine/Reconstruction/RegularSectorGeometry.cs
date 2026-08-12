namespace GWGUI.MediaEngine.Reconstruction;

/// <summary>Décrit une image sectorielle brute à géométrie régulière.</summary>
public sealed record RegularSectorGeometry(string FormatId, int BlockSize, int Cylinders, int Heads, int SectorsPerTrack, int FirstSectorNumber = 0)
{
    /// <summary>Obtient le nombre total de blocs logiques.</summary>
    public int BlockCount => checked(Cylinders * Heads * SectorsPerTrack);
    /// <summary>Obtient la capacité utile en octets.</summary>
    public int Capacity => checked(BlockCount * BlockSize);
}
