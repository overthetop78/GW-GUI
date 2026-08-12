namespace GWGUI.MediaEngine.Geometries.Ucsd;

/// <summary>Définit la géométrie logique des images UCSD au format IBM MFM.</summary>
internal static class UcsdIbmMfmGeometry
{
    /// <summary>Nombre de faces de l'image.</summary>
    public const int HeadCount = 1;

    /// <summary>Nombre de secteurs logiques par cylindre.</summary>
    public const int LogicalSectorsPerCylinder = 8;
}
