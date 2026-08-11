namespace GWGUI.MediaEngine.Primitives;

/// <summary>Regroupe les dimensions géométriques conventionnelles réellement partagées par plusieurs familles de médias.</summary>
internal static class DiskGeometryConstants
{
    /// <summary>Nombre de cylindres d'une géométrie conventionnelle à 40 pistes.</summary>
    public const int FortyTrackCylinderCount = 40;

    /// <summary>Nombre de cylindres d'une géométrie conventionnelle à 80 pistes.</summary>
    public const int EightyTrackCylinderCount = 80;

    /// <summary>Nombre de têtes d'un média simple face.</summary>
    public const int SingleSidedHeadCount = 1;

    /// <summary>Nombre de têtes d'un média double face.</summary>
    public const int DoubleSidedHeadCount = 2;
}
