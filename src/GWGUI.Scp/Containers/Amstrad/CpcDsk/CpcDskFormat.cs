namespace GWGUI.Scp.Containers.Amstrad.CpcDsk;

/// <summary>
/// Regroupe les signatures et l’identifiant technique du format de conteneur CPCEMU DSK.
/// </summary>
public static class CpcDskFormat
{
    /// <summary>Signature ASCII placée au début d’un conteneur CPCEMU DSK Standard.</summary>
    public const string StandardSignature = "MV - CPC";

    /// <summary>Signature ASCII placée au début d’un conteneur CPCEMU DSK Extended.</summary>
    public const string ExtendedSignature = "EXTENDED CPC DSK File";

    /// <summary>Signature ASCII placée au début de chaque bloc d’informations de piste.</summary>
    public const string TrackSignature = "Track-Info";

    /// <summary>Identifiant technique neutre d’une image sectorielle extraite d’un conteneur CPCEMU DSK.</summary>
    public const string FormatId = "cpcemu.dsk";
}
