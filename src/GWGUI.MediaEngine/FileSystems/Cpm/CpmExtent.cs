namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Représente un extent de fichier CP/M.</summary>
internal sealed record CpmExtent
{
    /// <summary>Crée un extent et copie ses allocations.</summary>
    public CpmExtent(byte user, string name, int number, byte recordCount, IEnumerable<int> allocations)
    {
        User = user; Name = name; Number = number; RecordCount = recordCount; Allocations = Array.AsReadOnly(allocations.ToArray());
    }

    /// <summary>Zone utilisateur.</summary>
    public byte User { get; }
    /// <summary>Nom complet du fichier.</summary>
    public string Name { get; }
    /// <summary>Numéro de l'extent.</summary>
    public int Number { get; }
    /// <summary>Nombre d'enregistrements utilisés.</summary>
    public byte RecordCount { get; }
    /// <summary>Allocations référencées.</summary>
    public IReadOnlyList<int> Allocations { get; }
}
