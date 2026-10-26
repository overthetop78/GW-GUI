namespace GWGUI.Emulation.Amstrad.Common.Constants;

internal static class EmulationModuleConstants
{
    /// <summary>Identifiant stable du module d’émulation.</summary>
    internal const string ModuleId = "amstrad";
    /// <summary>Clé de traduction du nom de la famille de machines du module.</summary>
    internal const string ResourceFamily = "Emulation.Family.Amstrad";
    /// <summary>Chemin du dossier des Assets externes du module, relatif au dossier racine de l’application.</summary>
    internal const string AssetPathPrefix = "Assets/Emulation/amstrad";

    /// <summary>Suffixe ajouté au dossier des Assets pour former le chemin de l’image de marque.</summary>
    internal const string ModuleIconPathSuffix = "/amstrad.png";

    /// <summary>Code de sortie initial du processus hôte du module.</summary>
    internal const int DefaultHostExitCode = 0;
    /// <summary>Nom du sous-dossier des cœurs d’émulation.</summary>
    internal const string CoreDirectoryName = "Core";
}
