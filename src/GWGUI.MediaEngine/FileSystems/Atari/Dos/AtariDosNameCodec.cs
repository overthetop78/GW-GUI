namespace GWGUI.MediaEngine.FileSystems.Atari.Dos;

/// <summary>Décode un nom Atari DOS au format 8.3.</summary>
public static class AtariDosNameCodec
{
    /// <summary>Décode le radical et ajoute le point uniquement lorsque l'extension existe.</summary>
    public static string Decode(ReadOnlySpan<byte> raw)
    {
        var name = System.Text.Encoding.ASCII.GetString(raw[..AtariDosFileSystemLayout.NameLength]).Trim();
        var extension = System.Text.Encoding.ASCII.GetString(raw[AtariDosFileSystemLayout.NameLength..]).Trim();
        return extension.Length == 0 ? name : $"{name}.{extension}";
    }
}
