using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Exploration.Metadata;

/// <summary>Construit les métadonnées techniques d'une image sans produire de texte d'interface.</summary>
internal sealed class DiskImageMetadataFactory(DiskSystemResolver systemResolver, DiskProtectionResolver protectionResolver)
{
    private readonly DiskContentDetector contentDetector = new();
    /// <summary>Agrège les formats dans leur ordre, supprime leurs doublons sans tenir compte de la casse et résout des identifiants copiés.</summary>
    /// <param name="image">Image sectorielle principale.</param>
    /// <param name="detectedFormatIds">Identifiants supplémentaires détectés.</param>
    /// <returns>Métadonnées techniques indépendantes des collections sources.</returns>
    public DiskImageMetadata Create(SectorImage image, IEnumerable<string>? detectedFormatIds = null)
    {
        var formatIds = new[] { image.FormatId }.Concat(detectedFormatIds ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var systems = formatIds.Select(systemResolver.ResolveId).Where(id => id is not null).Cast<string>().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        return new(systems, protectionResolver.ResolveId(formatIds), contentDetector.Analyze(image));
    }
}
