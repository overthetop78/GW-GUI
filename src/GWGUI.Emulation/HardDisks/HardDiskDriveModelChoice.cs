namespace GWGUI.Emulation.HardDisks;

/// <summary>Profil physique HDD fourni par MediaEngine au module via le SDK.</summary>
public sealed record HardDiskDriveModelChoice(string Id, string DisplayName, DiskImageGeometry Geometry)
{
    /// <summary>Autorise une géométrie différente de la présélection.</summary>
    public bool AllowsCustomGeometry { get; init; }
    /// <summary>Limite de capacité du profil, ou capacité du mécanisme fixe.</summary>
    public long MaximumBytes { get; init; } = Geometry.Capacity;
}
