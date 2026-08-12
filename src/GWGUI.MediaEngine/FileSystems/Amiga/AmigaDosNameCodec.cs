namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Décode les chaînes préfixées par leur longueur utilisées par AmigaDOS.</summary>
public static class AmigaDosNameCodec
{
    /// <summary>Lit une B-string Latin-1 dans la plage demandée.</summary>
    public static string Read(ReadOnlySpan<byte> block, int offset, int maximum)
    {
        if (offset < 0 || maximum < 0 || offset >= block.Length) return string.Empty;
        var length = Math.Min(block[offset], Math.Min(maximum, block.Length - offset - 1));
        return System.Text.Encoding.Latin1.GetString(block.Slice(offset + 1, length)).TrimEnd('\0');
    }

    /// <summary>Lit le nom ordinaire ou long autorisé par la variante.</summary>
    public static string ReadEntryName(ReadOnlySpan<byte> block, AmigaDosVariant variant)
    {
        var ordinary = Read(block, AmigaDosLayout.OrdinaryNameOffset, AmigaDosLayout.OrdinaryNameMaximumLength);
        return ordinary.Length != 0 || !variant.SupportsLongNames() ? ordinary : Read(block, AmigaDosLayout.LongNameOffset, AmigaDosLayout.LongNameMaximumLength);
    }
}
