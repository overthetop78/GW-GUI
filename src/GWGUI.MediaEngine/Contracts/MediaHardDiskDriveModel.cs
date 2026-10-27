namespace GWGUI.MediaEngine.Contracts;

/// <summary>Identité et géométrie d'un mécanisme HDD, indépendantes de l'émulateur consommateur.</summary>
public sealed record MediaHardDiskDriveModel(string Id, string DisplayName, HardDiskGeometry Geometry)
{
    /// <summary>Autorise une géométrie différente de la présélection.</summary>
    public bool AllowsCustomGeometry { get; init; }
    /// <summary>Limite de capacité du profil, ou capacité du mécanisme fixe.</summary>
    public long MaximumBytes { get; init; } = Geometry.Capacity;
}
