using System.Collections.Frozen;
using System.IO;

namespace GWGUI.Emulation.HardDisks;

/// <summary>A machine's supported combination of container, attachment and capacity policy.</summary>
public sealed record HardDiskImageFormat(string Id, string Extension, string InterfaceName,
    long MaximumBytes, long DefaultBytes, IReadOnlyList<HardDiskPreparation>? Preparations = null,
    Containers.DiskContainerKind Container = Containers.DiskContainerKind.Raw)
{
    public long MinimumBytes { get; init; } = 512;

    /// <summary>Exact native capacities, or null when the other size limits suffice.</summary>
    public IReadOnlyList<long>? AllowedCapacitiesBytes { get; init; }

    public IReadOnlySet<int> LogicalSectorSizes { get; init; } = new[] { 512 }.ToFrozenSet();

    public DiskChsGeometry? Geometry { get; init; }
    /// <summary>Autorise le dialogue à transmettre une géométrie complète et à dimensionner des cylindres entiers.</summary>
    public bool SupportsExplicitGeometry { get; init; }

    /// <summary>Créateur du profil lorsque son conteneur nécessite une publication particulière, notamment multifichier.</summary>
    /// <remarks>Le créateur publie ses fichiers sans remplacer de destination existante et libère ses ressources.</remarks>
    public Action<string, long, bool, HardDiskPreparation, DiskImageGeometry?>? ImageCreator { get; init; }

    public DiskFormatOperations Operations { get; init; } = DiskFormatOperations.Create | DiskFormatOperations.Read;

    public bool SupportsFixedAllocation { get; init; } = true;

    /// <summary>Initializes a newly created blank image. The caller owns the stream.</summary>
    public Action<Stream>? InitializeBlankImage { get; init; }

    public IReadOnlyList<HardDiskPreparation> SupportedPreparations => Preparations ?? [HardDiskPreparation.Blank];

    public string DisplayName => $"{InterfaceName} (*{Extension})";
}
