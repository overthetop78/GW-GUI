namespace GWGUI.Emulation.Acorn.Common.Constants;

internal static class EmulationModuleConstants
{
    /// <summary>Identifiant stable du module d’émulation.</summary>
    internal const string ModuleId = "acorn";
    /// <summary>Clé de traduction du nom de la famille de machines du module.</summary>
    internal const string ResourceFamily = "Emulation.Family.Acorn";
    /// <summary>Chemin du dossier des Assets externes du module, relatif au dossier racine de l’application.</summary>
    internal const string AssetPathPrefix = "Assets/Emulation/acorn";
    /// <summary>Chemin relatif de l’image BBC Micro partagée entre les modèles A et B.</summary>
    internal const string BbcMicroImagePath = AssetPathPrefix + "/Machines/BbcMicro.png";
    /// <summary>Préfixe des ressources de traduction intégrées au module.</summary>
    internal const string LocalizationResourceName = "GWGUI.Emulation.Acorn.Resources.Emulation";
    /// <summary>Nom du sous-dossier des configurations de machines du module.</summary>
    internal const string ConfigurationsDirectoryName = "Configurations";
    /// <summary>Nom du sous-dossier des cœurs d’émulation.</summary>
    internal const string CoreDirectoryName = "Core";
}

