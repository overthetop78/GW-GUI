namespace GWGUI.MediaEngine.Encoding;

/// <summary>Définit les limites communes appliquées aux requêtes d'encodage de piste.</summary>
internal static class TrackEncodingLimits
{
    /// <summary>Plus petit numéro de cylindre admis.</summary>
    public const int MinimumCylinder = 0;
    /// <summary>Plus grand numéro de cylindre admis.</summary>
    public const int MaximumCylinder = byte.MaxValue;
    /// <summary>Plus petit numéro de face admis.</summary>
    public const int MinimumHead = 0;
    /// <summary>Plus grand numéro de face admis.</summary>
    public const int MaximumHead = 1;
    /// <summary>Nombre minimal de secteurs nécessaire pour encoder une piste.</summary>
    public const int MinimumSectorCount = 1;
    /// <summary>Plus petit code de taille sectorielle ISO admis.</summary>
    public const byte MinimumSectorSizeCode = Primitives.SectorSizeCode.MinimumCode;
    /// <summary>Plus grand code de taille sectorielle ISO admis.</summary>
    public const byte MaximumSectorSizeCode = Primitives.SectorSizeCode.MaximumCode;
}
