namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Définit la disposition binaire de la table linéaire de ressources.</summary>
internal static class AmigaFlatResourceArchiveLayout
{
    public const int DirectoryStartBlock = 2;
    public const int NameLength = 12;
    public const int EntryLength = 16;
    public const int SizeOffset = 12;
    public const string ReservedName = "Reserved";
    public const string EntryComment = "Flat resource archive entry";
}
