namespace GWGUI.Emulation.Nec.Common.Constants;

/// <summary>Conventions des fichiers et métadonnées d'installation du catalogue.</summary>
internal static class EmulatorCatalogConstants
{
    /// <summary>Nom du manifeste écrit à côté de la bibliothèque installée.</summary>
    internal const string ManifestFileName = "core.json";
    /// <summary>Propriété JSON contenant la version de cette installation.</summary>
    internal const string VersionPropertyName = "version";
    /// <summary>Architecture annoncée pour la bibliothèque Windows x64 validée par le service.</summary>
    internal const string WindowsX64Architecture = "x64";
}
