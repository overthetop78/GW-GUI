namespace GWGUI.MediaEngine.FileSystems.Acorn.FileCore;

/// <summary>Décode les noms ASCII fixes d'un DiscRecord FileCore.</summary>
public static class AcornFileCoreNameCodec
{
    /// <summary>Décode un nom et supprime les terminateurs et remplissages finaux.</summary>
    public static string Decode(ReadOnlySpan<byte> data) => System.Text.Encoding.ASCII.GetString(data).TrimEnd('\0', '\r', ' ');
}
