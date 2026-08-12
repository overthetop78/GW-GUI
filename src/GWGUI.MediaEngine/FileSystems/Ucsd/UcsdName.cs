namespace GWGUI.MediaEngine.FileSystems.Ucsd;

/// <summary>Valide et décode les noms Pascal UCSD.</summary>
internal static class UcsdName
{
    /// <summary>Décode un champ de nom borné ou retourne une chaîne vide s'il est invalide.</summary>
    public static string Decode(ReadOnlySpan<byte> field, int maximumLength)
    {
        if (field.Length == 0) return string.Empty;
        var length = field[0];
        if (length == 0 || length > maximumLength || length >= field.Length || !IsValid(field.Slice(1, length))) return string.Empty;
        return System.Text.Encoding.ASCII.GetString(field.Slice(1, length));
    }

    /// <summary>Indique si chaque caractère appartient à la plage ASCII imprimable.</summary>
    public static bool IsValid(ReadOnlySpan<byte> name)
    {
        if (name.Length == 0) return false;
        foreach (var value in name) if (value is < UcsdFileSystemLayout.MinimumNameCharacter or >= UcsdFileSystemLayout.MaximumNameCharacterExclusive) return false;
        return true;
    }
}
