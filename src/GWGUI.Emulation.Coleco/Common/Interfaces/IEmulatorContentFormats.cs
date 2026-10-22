namespace GWGUI.Emulation.Coleco.Common.Interfaces;

/// <summary>Adaptation des extensions annoncées par le cœur aux contraintes de chargement du backend.</summary>
internal interface IEmulatorContentFormats
{
    /// <summary>Résout les extensions utilisables sans ouvrir ni convertir de fichier.</summary>
    /// <param name="declaredExtensions">Extensions empruntées au cœur, conservées intactes.</param>
    /// <param name="requiresFilePath">True si le cœur demande un chemin plutôt que les octets du contenu.</param>
    /// <returns>Extensions acceptées après application des règles existantes du backend.</returns>
    IReadOnlySet<string> ResolveContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath);
}
