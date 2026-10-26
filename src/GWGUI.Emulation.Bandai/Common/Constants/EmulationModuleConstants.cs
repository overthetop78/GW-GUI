namespace GWGUI.Emulation.Bandai.Common.Constants;

internal static class EmulationModuleConstants
{
    /// <summary>Identifiant stable du module d’émulation.</summary>
    internal const string ModuleId = "bandai";
    /// <summary>Clé de traduction du nom de la famille de machines du module.</summary>
    internal const string ResourceFamily = "Emulation.Family.Bandai";
    /// <summary>Chemin du dossier des Assets externes du module, relatif au dossier racine de l’application.</summary>
    internal const string AssetPathPrefix = "Assets/Emulation/bandai";
    /// <summary>Préfixe des ressources de traduction intégrées au module.</summary>
    internal const string LocalizationResourceName = "GWGUI.Emulation.Bandai.Resources.Emulation";
    /// <summary>Nom du sous-dossier des configurations de machines du module.</summary>
    internal const string ConfigurationsDirectoryName = "Configurations";
    /// <summary>Nom du sous-dossier des cœurs d’émulation.</summary>
    internal const string CoreDirectoryName = "Core";
    /// <summary>Nom du sous-dossier des sauvegardes des machines du module.</summary>
    internal const string SavesDirectoryName = "Saves";
}
