namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Centralise les erreurs paramétrées du lecteur d'archive plate.</summary>
internal static class AmigaFlatResourceArchiveExceptions
{
    public static InvalidDataException InvalidDirectory() => new("The image does not contain a valid Amiga flat-resource archive directory.");
    public static InvalidDataException RangeOutsideImage(long offset, long length, long capacity) =>
        new($"Flat-archive range {offset}+{length} exceeds the image capacity {capacity}.");
}
