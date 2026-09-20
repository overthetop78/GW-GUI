using GWGUI.MediaEngine.Contracts.Explorer;

namespace GWGUI.MediaEngine.Exploration.Interpretation;

/// <summary>Détermine si une interprétation secondaire est suffisamment crédible pour être présentée.</summary>
internal static class FileSystemAlternativePolicy
{
    /// <summary>Seuil minimal d'avertissements tolérés indépendamment du nombre d'entrées.</summary>
    public const int MinimumWarningThreshold = 3;
    /// <summary>Nombre minimal d'entrées de catalogue contrôlées attestant un volume nommé malgré des données manquantes.</summary>
    public const int MinimumValidatedCatalogEntries = 3;
    /// <summary>Indique si les avertissements restent proportionnés au catalogue ou si plusieurs entrées contrôlées attestent le volume.</summary>
    /// <param name="volume">Volume alternatif à évaluer.</param>
    /// <returns><see langword="true"/> lorsque l'alternative reste crédible.</returns>
    public static bool IsCredible(FileSystemVolume volume)
    {
        var entries = Enumerate(volume.Entries).ToArray();
        if (volume.Warnings.Count <= Math.Max(MinimumWarningThreshold, entries.Length)) return true;
        return entries.Count(entry => entry.MetadataValid && !entry.SyntheticName && !string.IsNullOrWhiteSpace(entry.Name)) >= MinimumValidatedCatalogEntries;
    }

    private static IEnumerable<FileSystemEntry> Enumerate(IEnumerable<FileSystemEntry> entries)
    {
        foreach (var entry in entries)
        {
            yield return entry;
            foreach (var child in Enumerate(entry.Children)) yield return child;
        }
    }
}
