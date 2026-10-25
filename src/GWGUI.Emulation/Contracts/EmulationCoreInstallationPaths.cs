namespace GWGUI.Emulation.Contracts;

/// <summary>Emplacements et version d'une installation de cœur résolue par le service Common du module.</summary>
/// <param name="VersionDirectory">Dossier contenant cette installation ; son nom ne constitue pas un numéro de version.</param>
/// <param name="LibraryPath">Bibliothèque utilisable par le fournisseur du cœur.</param>
/// <param name="ManifestPath">Manifeste de l'installation, ou chaîne vide pour une implémentation intégrée au build.</param>
public sealed record EmulationCoreInstallationPaths(
    string VersionDirectory,
    string LibraryPath,
    string ManifestPath)
{
    /// <summary>Version déterminée par le marqueur actif, le manifeste ou le build intégré ; null si ces chemins ne décrivent pas encore une installation résolue.</summary>
    public string? InstalledVersion { get; init; }
}
