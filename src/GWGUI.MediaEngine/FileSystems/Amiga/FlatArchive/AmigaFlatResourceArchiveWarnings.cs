namespace GWGUI.MediaEngine.FileSystems.Amiga.FlatArchive;

/// <summary>Centralise les avertissements techniques du lecteur d'archive plate.</summary>
internal static class AmigaFlatResourceArchiveWarnings
{
    public static string MissingBlocks(string name, IReadOnlyList<int> blocks) =>
        $"{name}: missing source block(s) {string.Join(", ", blocks)}; unavailable bytes were replaced with zeroes.";
    public static string InvalidBlocks(string name, IReadOnlyList<int> blocks) =>
        $"{name}: checksum-invalid source block(s) {string.Join(", ", blocks)}.";
}
