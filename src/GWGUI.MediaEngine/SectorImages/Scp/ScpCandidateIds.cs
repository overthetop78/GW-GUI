namespace GWGUI.MediaEngine.SectorImages.Scp;

/// <summary>Définit les identifiants techniques des reconstructeurs SCP composés.</summary>
internal static class ScpCandidateIds
{
    public const string IsoAutomatic = "scp.iso.auto";
    public const string IsoSelected = "scp.iso.selected";
    public const string Amiga = "scp.amiga";
    public const string Atari = "scp.atari";
    public const string AtariSt720 = "scp.atari-st-720";
    public const string CommodoreAutomatic = "scp.commodore.auto";
    public const string Commodore1581 = "scp.commodore.1581";
    public const string Apple = "scp.apple";
    public const string Dec = "scp.dec";
    public static string IsoFormat(string formatId) => $"scp.iso.{formatId}";
}
