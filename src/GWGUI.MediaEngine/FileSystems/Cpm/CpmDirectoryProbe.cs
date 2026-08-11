namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Recherche et valide les entrées structurelles d'un répertoire CP/M.</summary>
internal static class CpmDirectoryProbe
{
    /// <summary>Recherche la disposition CPC brute actuellement prise en charge.</summary>
    /// <param name="bytes">Données sectorielles dans leur ordre logique.</param>
    /// <returns>Disposition reconnue, ou <see langword="null"/>.</returns>
    public static CpmDirectoryLayout? FindCpcRawDirectory(byte[] bytes) => FindDirectory(bytes, 64, 1024, 2, false);

    /// <summary>Recherche un répertoire CP/M à chaque frontière sectorielle de 512 octets.</summary>
    /// <param name="bytes">Données sectorielles dans leur ordre logique.</param>
    /// <param name="entries">Nombre d'entrées attendu.</param>
    /// <param name="allocationSize">Taille d'un bloc d'allocation.</param>
    /// <param name="directoryBlocks">Nombre de blocs réservés au répertoire.</param>
    /// <param name="wide">Indique si les numéros de blocs occupent deux octets.</param>
    /// <returns>Première disposition plausible, ou <see langword="null"/>.</returns>
    public static CpmDirectoryLayout? FindDirectory(byte[] bytes, int entries, int allocationSize, int directoryBlocks, bool wide)
    {
        for (var offset = 0; offset + entries * 32 <= bytes.Length; offset += 512)
        {
            var layout = new CpmDirectoryLayout(offset, offset, entries, allocationSize, directoryBlocks, wide);
            if (LooksLikeDirectory(bytes, layout)) return layout;
        }
        return null;
    }

    /// <summary>Vérifie que la plage décrite contient uniquement des entrées CP/M plausibles.</summary>
    /// <param name="bytes">Données sectorielles contenant le répertoire.</param>
    /// <param name="layout">Disposition à vérifier.</param>
    /// <param name="allowEmpty">Autorise un répertoire entièrement inutilisé.</param>
    /// <returns><see langword="true"/> lorsque toutes les entrées sont plausibles.</returns>
    public static bool LooksLikeDirectory(byte[] bytes, CpmDirectoryLayout layout, bool allowEmpty = false)
    {
        if (layout.DirectoryOffset < 0 || layout.DirectoryOffset + layout.DirectoryEntries * 32 > bytes.Length) return false;
        var active = 0;
        var unused = 0;
        for (var index = 0; index < layout.DirectoryEntries; index++)
        {
            var entry = bytes.AsSpan(layout.DirectoryOffset + index * 32, 32);
            if (entry[0] == 0xe5) { unused++; continue; }
            if (entry[0] <= 31 && HasValidName(entry)) active++;
            else if (entry[0] is not (0x20 or 0x21)) return false;
        }
        return active > 0 || allowEmpty && unused == layout.DirectoryEntries;
    }

    /// <summary>Vérifie les caractères du nom et la présence d'un radical non vide.</summary>
    /// <param name="entry">Entrée CP/M de 32 octets.</param>
    /// <returns><see langword="true"/> lorsque son nom est valide.</returns>
    private static bool HasValidName(ReadOnlySpan<byte> entry)
    {
        for (var index = 1; index <= 11; index++)
        {
            var value = entry[index] & 0x7f;
            if (value != 0x20 && (value < 0x21 || value > 0x7e)) return false;
        }
        for (var index = 1; index <= 8; index++) if ((entry[index] & 0x7f) != 0x20) return true;
        return false;
    }
}
