namespace GWGUI.MediaEngine.Containers.Apple.DiskCopy;

/// <summary>Distingue les géométries déduites du nombre de blocs d'un conteneur DiskCopy tagué.</summary>
internal enum DiskCopyTaggedGeometryKind
{
    /// <summary>Disquette Lisa FileWare.</summary>
    LisaFileWare,
    /// <summary>Disquette Macintosh GCR simple face de 400 Kio.</summary>
    Macintosh400K,
    /// <summary>Disquette Macintosh GCR double face de 800 Kio.</summary>
    Macintosh800K,
    /// <summary>Géométrie taguée non spécialisée.</summary>
    Generic
}
