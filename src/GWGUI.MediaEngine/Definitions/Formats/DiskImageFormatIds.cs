namespace GWGUI.MediaEngine.Definitions;

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
    public const string CpcEmuDsk = "cpcemu.dsk";
}
