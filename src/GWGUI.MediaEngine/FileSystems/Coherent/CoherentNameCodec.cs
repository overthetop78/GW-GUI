namespace GWGUI.MediaEngine.FileSystems.Coherent;

/// <summary>Décode les champs ASCII fixes du système COHERENT.</summary>
public static class CoherentNameCodec
{
    /// <summary>Retire les terminateurs nul, espace, saut de ligne et retour chariot.</summary>
    public static string Decode(ReadOnlySpan<byte> bytes) => System.Text.Encoding.ASCII.GetString(bytes).TrimEnd('\0', ' ', '\n', '\r');
    /// <summary>Construit la description technique d'un inode.</summary>
    public static string InodeDescription(ushort number) => $"COHERENT inode {number}";
}
