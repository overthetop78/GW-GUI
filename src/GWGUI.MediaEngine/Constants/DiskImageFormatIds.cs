using MediaImageFormatIds = global::GWGUI.MediaFileSystems.Constants.MediaImageFormatIds;

namespace GWGUI.MediaEngine.Constants;

/// <summary>Regroupe les identifiants publics des formats d'images reconnus par le moteur.</summary>
public static partial class DiskImageFormatIds
{
    /// <summary>Identifiant utilisé lorsqu'aucun format n'a pu être déterminé.</summary>
    public const string Unknown = "unknown";
    /// <summary>Identifiant générique des conteneurs ImageDisk.</summary>
    public const string Imd = "imd";
    /// <summary>Identifiant générique des conteneurs Teledisk.</summary>
    public const string Td0 = "td0";
    /// <summary>Identifiant neutre des conteneurs CPCEMU DSK.</summary>
    public const string CpcEmuDsk = MediaImageFormatIds.CpcEmuDsk;
    /// <summary>Identifiant du conteneur de pistes HxC Floppy Emulator.</summary>
    public const string RawHfe = "raw.hfe";
    /// <summary>Identifiant du conteneur de flux SuperCard Pro.</summary>
    public const string RawScp = "raw.scp";
    /// <summary>Identifiant des captures de flux multipistes HxC Stream.</summary>
    public const string RawHxcStream = "raw.hxcstream";
    /// <summary>Identifiant des captures de flux multipistes KryoFlux.</summary>
    public const string RawKryoFlux = "raw.kryoflux";
    /// <summary>Identifiant des cartouches Amstrad Plus/GX4000.</summary>
    public const string AmstradCpr = MediaImageFormatIds.AmstradCpr;
    /// <summary>Identifiant des ROM brutes Amstrad CPC.</summary>
    public const string AmstradRom = MediaImageFormatIds.AmstradRom;
    /// <summary>Identifiant des conteneurs ApriDisk ACT Apricot PC/Xi de 315 Kio.</summary>
    public const string ApricotPcXi315 = MediaImageFormatIds.ApricotPcXi315;
}
