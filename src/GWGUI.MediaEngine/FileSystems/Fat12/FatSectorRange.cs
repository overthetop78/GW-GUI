namespace GWGUI.MediaEngine.FileSystems.Fat12;

/// <summary>Conserve les octets positionnés d'une plage de secteurs et la présence de chaque secteur.</summary>
internal sealed class FatSectorRange
{
    /// <summary>Crée le résultat en copiant les octets et le masque de présence.</summary>
    public FatSectorRange(byte[] bytes, bool[] presentSectors)
    {
        Bytes = bytes.ToArray();
        PresentSectors = Array.AsReadOnly(presentSectors.ToArray());
    }

    /// <summary>Octets positionnés, avec des zéros réservés aux secteurs absents.</summary>
    public byte[] Bytes { get; }
    /// <summary>Présence réelle de chaque secteur demandé.</summary>
    public IReadOnlyList<bool> PresentSectors { get; }
    /// <summary>Indique si tous les secteurs étaient présents et de la bonne taille.</summary>
    public bool IsValid => PresentSectors.All(value => value);
}
