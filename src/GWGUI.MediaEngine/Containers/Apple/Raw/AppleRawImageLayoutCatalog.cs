using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Containers.Apple.Raw;

/// <summary>Associe chaque capacité Apple brute connue aux géométries qu'elle peut représenter.</summary>
internal static class AppleRawImageLayoutCatalog
{
    /// <summary>Image Apple II DOS 3.2 à treize secteurs.</summary>
    public static AppleRawImageLayout D13 { get; } = new("D13", AppleIIGeometry.Dos32Capacity, [AppleRawGeometryKind.AppleII525Dos32]);
    /// <summary>Image Apple II 140 Kio en ordre DOS ou ProDOS.</summary>
    public static AppleRawImageLayout AppleII140K { get; } = new("Apple II 140 K", AppleIIGeometry.Capacity, [AppleRawGeometryKind.AppleII525Dos33, AppleRawGeometryKind.AppleII525ProDos]);
    /// <summary>Image Apple 400 Kio pouvant représenter Lisa ou Macintosh GCR simple face.</summary>
    public static AppleRawImageLayout Apple400K { get; } = new("Apple 400 K", MacintoshGcrGeometry.Capacity400K, [AppleRawGeometryKind.LisaFileWare, AppleRawGeometryKind.MacintoshGcr400]);
    /// <summary>Image Apple 800 Kio pouvant représenter Macintosh GCR ou ProDOS.</summary>
    public static AppleRawImageLayout Apple800K { get; } = new("Apple 800 K", MacintoshGcrGeometry.Capacity800K, [AppleRawGeometryKind.MacintoshGcr800, AppleRawGeometryKind.ProDos800]);
    /// <summary>Image Macintosh MFM 1,44 Mio.</summary>
    public static AppleRawImageLayout Macintosh1440K { get; } = new("Macintosh 1.44 M", MacintoshMfmGeometry.Capacity, [AppleRawGeometryKind.MacintoshMfm1440]);
    /// <summary>Toutes les dispositions cataloguées dans l'ordre de sélection.</summary>
    public static IReadOnlyList<AppleRawImageLayout> All { get; } = Array.AsReadOnly(new[] { D13, AppleII140K, Apple400K, Apple800K, Macintosh1440K });

    /// <summary>Recherche la disposition possédant exactement la capacité indiquée.</summary>
    public static AppleRawImageLayout? Find(int capacity) => All.SingleOrDefault(layout => layout.Capacity == capacity);
}

/// <summary>Décrit une capacité et toutes ses interprétations géométriques possibles.</summary>
internal sealed record AppleRawImageLayout(string Name, int Capacity, IReadOnlyList<AppleRawGeometryKind> Geometries);

/// <summary>Identifie les géométries possibles sans leur attribuer prématurément un système de fichiers.</summary>
internal enum AppleRawGeometryKind
{
    /// <summary>Apple II 5,25 pouces DOS 3.2.</summary>
    AppleII525Dos32,
    /// <summary>Apple II 5,25 pouces DOS 3.3.</summary>
    AppleII525Dos33,
    /// <summary>Apple II 5,25 pouces en ordre ProDOS.</summary>
    AppleII525ProDos,
    /// <summary>Lisa FileWare 400 Kio.</summary>
    LisaFileWare,
    /// <summary>Macintosh GCR simple face 400 Kio.</summary>
    MacintoshGcr400,
    /// <summary>Macintosh GCR double face 800 Kio.</summary>
    MacintoshGcr800,
    /// <summary>ProDOS 800 Kio.</summary>
    ProDos800,
    /// <summary>Macintosh MFM 1,44 Mio.</summary>
    MacintoshMfm1440
}
