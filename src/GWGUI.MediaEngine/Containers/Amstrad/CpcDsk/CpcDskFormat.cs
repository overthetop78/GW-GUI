using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>
/// Regroupe les signatures et l’identifiant technique du format de conteneur CPCEMU DSK.
/// </summary>
public static class CpcDskFormat
{
    /// <summary>Signature ASCII placée au début d’un conteneur CPCEMU DSK Standard.</summary>
    public const string StandardSignature = "MV - CPC";
    /// <summary>Signature binaire immuable d'un conteneur CPCEMU DSK Standard.</summary>
    public static ReadOnlySpan<byte> StandardSignatureBytes => "MV - CPC"u8;

    /// <summary>Signature ASCII placée au début d’un conteneur CPCEMU DSK Extended.</summary>
    public const string ExtendedSignature = "EXTENDED CPC DSK File";
    /// <summary>Signature binaire immuable d'un conteneur CPCEMU DSK Extended.</summary>
    public static ReadOnlySpan<byte> ExtendedSignatureBytes => "EXTENDED CPC DSK File"u8;

    /// <summary>Signature ASCII placée au début de chaque bloc d’informations de piste.</summary>
    public const string TrackSignature = "Track-Info";
    /// <summary>Signature binaire immuable d'un bloc d'informations de piste.</summary>
    public static ReadOnlySpan<byte> TrackSignatureBytes => "Track-Info"u8;

    /// <summary>Identifiant technique neutre d’une image sectorielle extraite d’un conteneur CPCEMU DSK.</summary>
    public const string FormatId = DiskImageFormatIds.CpcEmuDsk;
}
