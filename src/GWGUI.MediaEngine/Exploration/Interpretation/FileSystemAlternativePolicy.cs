using GWGUI.MediaEngine.FileSystems;

namespace GWGUI.MediaEngine.Exploration.Interpretation;

/// <summary>Détermine si une interprétation secondaire est suffisamment crédible pour être présentée.</summary>
internal static class FileSystemAlternativePolicy
{
    /// <summary>Seuil minimal d'avertissements tolérés indépendamment du nombre d'entrées.</summary>
    public const int MinimumWarningThreshold = 3;
    /// <summary>Indique si les avertissements ne dépassent pas le maximum du seuil minimal et du nombre d'entrées.</summary>
    /// <param name="volume">Volume alternatif à évaluer.</param>
    /// <returns><see langword="true"/> lorsque l'alternative reste crédible.</returns>
    public static bool IsCredible(FileSystemVolume volume) => volume.Warnings.Count <= Math.Max(MinimumWarningThreshold, volume.Entries.Count);
}
