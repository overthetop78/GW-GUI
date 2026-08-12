using System.Collections.ObjectModel;

namespace GWGUI.MediaEngine.Geometries.Apple;

/// <summary>Définit la géométrie et les ordres sectoriels Apple II 5,25 pouces.</summary>
public static class AppleIIGeometry
{
    /// <summary>Taille d'un secteur en octets.</summary>
    public const int SectorSize = 256;
    /// <summary>Nombre de secteurs par piste, numérotés à partir de zéro.</summary>
    public const int SectorsPerTrack = 16;
    /// <summary>Taille d'une piste en octets.</summary>
    public const int TrackSize = SectorSize * SectorsPerTrack;
    /// <summary>Nombre de pistes d'une image standard.</summary>
    public const int TrackCount = 35;
    /// <summary>Capacité d'une image standard en octets.</summary>
    public const int Capacity = TrackCount * TrackSize;
    /// <summary>Associe chaque numéro de secteur logique ProDOS, à base zéro, à son numéro de secteur physique.</summary>
    public static ReadOnlyCollection<int> ProDosToPhysical { get; } = Array.AsReadOnly(new[] { 0, 2, 4, 6, 8, 10, 12, 14, 1, 3, 5, 7, 9, 11, 13, 15 });
    /// <summary>Associe chaque numéro de secteur physique, à base zéro, à sa position dans un fichier DOS.</summary>
    public static ReadOnlyCollection<int> PhysicalToDos { get; } = Array.AsReadOnly(new[] { 0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15 });

    /// <summary>Valide les tables d'ordre sectoriel lors de leur première utilisation.</summary>
    static AppleIIGeometry()
    {
        ValidatePermutation(ProDosToPhysical);
        ValidatePermutation(PhysicalToDos);
    }

    /// <summary>Vérifie qu'une table contient chaque secteur exactement une fois.</summary>
    private static void ValidatePermutation(IEnumerable<int> values)
    {
        if (!values.Order().SequenceEqual(Enumerable.Range(0, SectorsPerTrack))) throw new InvalidOperationException("Apple II sector-order table is not a complete permutation.");
    }
}
