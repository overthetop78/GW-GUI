namespace GWGUI.MediaEngine.FileSystems.Apple.Dos;

/// <summary>Décode les noms Apple DOS stockés en ASCII à bit fort.</summary>
public static class AppleDosNameCodec
{
    /// <summary>Retire le bit fort et les remplissages espace ou nul.</summary>
    public static string Decode(ReadOnlySpan<byte> raw)
    {
        Span<byte> decoded = stackalloc byte[raw.Length];
        for (var index = 0; index < raw.Length; index++) decoded[index] = (byte)(raw[index] & AppleDosFileSystemLayout.ValueMask);
        return System.Text.Encoding.ASCII.GetString(decoded).TrimEnd(' ', '\0');
    }
}
