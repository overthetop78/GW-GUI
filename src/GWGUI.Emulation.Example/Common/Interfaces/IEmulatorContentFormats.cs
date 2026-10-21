namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Filtrage des formats de contenu annoncés par le cœur selon ses contraintes de chargement.</summary>
/// <remarks>Cette opération adapte une description ; elle n’ouvre ni ne convertit les fichiers.</remarks>
internal interface IEmulatorContentFormats
{
    /// <summary>Résout les extensions effectivement utilisables à partir des déclarations natives.</summary>
    /// <param name="declaredExtensions">Ensemble emprunté d’extensions annoncé par le cœur.</param>
    /// <param name="requiresFilePath">True si le cœur exige un chemin plutôt qu’un contenu en mémoire.</param>
    /// <returns>Extensions conservées après application des contraintes de l’adaptateur.</returns>
    IReadOnlySet<string> ResolveContentExtensions(IReadOnlySet<string> declaredExtensions, bool requiresFilePath);
}
