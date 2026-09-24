namespace GWGUI.MediaEngine.Constants;

/// <summary>Regroupe les dimensions géométriques conventionnelles réellement partagées par plusieurs familles de médias.</summary>
internal static class DiskGeometryConstants
{
    /// <summary>Nombre de cylindres d'une géométrie conventionnelle à 40 pistes.</summary>
    public const int FortyTrackCylinderCount = GWGUI.MediaFileSystems.Constants.DiskGeometryConstants.FortyTrackCylinderCount;

    /// <summary>Nombre de cylindres d'une géométrie conventionnelle à 80 pistes.</summary>
    public const int EightyTrackCylinderCount = GWGUI.MediaFileSystems.Constants.DiskGeometryConstants.EightyTrackCylinderCount;

    /// <summary>Nombre de têtes d'un média simple face.</summary>
    public const int SingleSidedHeadCount = GWGUI.MediaFileSystems.Constants.DiskGeometryConstants.SingleSidedHeadCount;

    /// <summary>Nombre de têtes d'un média double face.</summary>
    public const int DoubleSidedHeadCount = GWGUI.MediaFileSystems.Constants.DiskGeometryConstants.DoubleSidedHeadCount;
}
