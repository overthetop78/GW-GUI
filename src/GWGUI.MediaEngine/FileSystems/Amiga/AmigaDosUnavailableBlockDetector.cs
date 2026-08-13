namespace GWGUI.MediaEngine.FileSystems.Amiga;

/// <summary>Reconnaît les blocs de remplacement explicitement écrits pour un secteur illisible.</summary>
internal static class AmigaDosUnavailableBlockDetector
{
    private static ReadOnlySpan<byte> BadSectorPattern => "-=[BAD SECTOR]=-"u8;

    /// <summary>Vérifie que le bloc entier est constitué de la répétition du marqueur d'indisponibilité.</summary>
    public static bool IsUnavailable(ReadOnlySpan<byte> block)
    {
        if (block.Length == 0 || block.Length % BadSectorPattern.Length != 0) return false;
        for (var offset = 0; offset < block.Length; offset += BadSectorPattern.Length)
            if (!block.Slice(offset, BadSectorPattern.Length).SequenceEqual(BadSectorPattern)) return false;
        return true;
    }
}
