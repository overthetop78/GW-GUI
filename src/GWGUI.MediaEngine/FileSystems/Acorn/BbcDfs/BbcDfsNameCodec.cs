namespace GWGUI.MediaEngine.FileSystems.Acorn.BbcDfs;

/// <summary>Décode les textes BBC DFS à sept bits.</summary>
public static class BbcDfsNameCodec
{
    /// <summary>Retire le bit fort et les remplissages nul et espace.</summary>
    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        Span<byte> clean = stackalloc byte[bytes.Length];
        for (var index = 0; index < bytes.Length; index++) clean[index] = (byte)(bytes[index] & BbcDfsFileSystemLayout.CharacterMask);
        return System.Text.Encoding.ASCII.GetString(clean).TrimEnd('\0', ' ');
    }
    /// <summary>Construit la description technique des adresses load et execute.</summary>
    public static string Description(int load, int execute) => $"DFS load &{load:X6}, execute &{execute:X6}";
}
