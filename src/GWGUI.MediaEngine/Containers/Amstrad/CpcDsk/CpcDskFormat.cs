using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;

/// <summary>
/// Regroupe les signatures et l’identifiant technique du format de conteneur CPCEMU DSK.
/// </summary>
public static class CpcDskFormat
{
    /// <summary>En-tête ASCII complet d'un conteneur CPCEMU DSK Standard.</summary>
    public const string StandardHeader = "MV - CPCEMU Disk-File\r\nDisk-Info\r\n";

    /// <summary>Signature ASCII placée au début d’un conteneur CPCEMU DSK Standard.</summary>
    public const string StandardSignature = "MV - CPC";
    /// <summary>Signature binaire immuable d'un conteneur CPCEMU DSK Standard.</summary>
    public static ReadOnlySpan<byte> StandardSignatureBytes => "MV - CPC"u8;

    /// <summary>En-tête binaire complet d'un conteneur CPCEMU DSK Standard.</summary>
    public static ReadOnlySpan<byte> StandardHeaderBytes => "MV - CPCEMU Disk-File\r\nDisk-Info\r\n"u8;

    /// <summary>En-tête ASCII complet d'un conteneur CPCEMU DSK Extended.</summary>
    public const string ExtendedHeader = "EXTENDED CPC DSK File\r\nDisk-Info\r\n";

    /// <summary>Signature ASCII placée au début d’un conteneur CPCEMU DSK Extended.</summary>
    public const string ExtendedSignature = "EXTENDED CPC DSK File";
    /// <summary>Signature binaire immuable d'un conteneur CPCEMU DSK Extended.</summary>
    public static ReadOnlySpan<byte> ExtendedSignatureBytes => "EXTENDED CPC DSK File"u8;

    /// <summary>En-tête binaire complet d'un conteneur CPCEMU DSK Extended.</summary>
    public static ReadOnlySpan<byte> ExtendedHeaderBytes => "EXTENDED CPC DSK File\r\nDisk-Info\r\n"u8;

    /// <summary>Signature ASCII placée au début de chaque bloc d’informations de piste.</summary>
    public const string TrackSignature = "Track-Info";
    /// <summary>Signature binaire immuable d'un bloc d'informations de piste.</summary>
    public static ReadOnlySpan<byte> TrackSignatureBytes => "Track-Info"u8;

    /// <summary>En-tête binaire complet d'un bloc d'informations de piste.</summary>
    public static ReadOnlySpan<byte> TrackHeaderBytes => "Track-Info\r\n"u8;

    /// <summary>Identifiant du logiciel créateur écrit dans le champ de quatorze octets.</summary>
    public const string Creator = "GW GUI";

    /// <summary>Valeur GAP#3 utilisée lorsqu'une image sectorielle ne fournit pas d'en-tête de piste.</summary>
    public const byte DefaultGap3Length = 0x4e;

    /// <summary>Octet de remplissage utilisé lorsqu'une image sectorielle ne fournit pas d'en-tête de piste.</summary>
    public const byte DefaultFillerByte = 0xe5;

    /// <summary>Identifiant technique neutre d’une image sectorielle extraite d’un conteneur CPCEMU DSK.</summary>
    public const string FormatId = DiskImageFormatIds.CpcEmuDsk;
}
