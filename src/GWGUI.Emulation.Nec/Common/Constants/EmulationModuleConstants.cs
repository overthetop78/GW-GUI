namespace GWGUI.Emulation.Nec.Common.Constants;

internal static class EmulationModuleConstants
{
    /// <summary>Identifiant stable du module d’émulation.</summary>
    internal const string ModuleId = "nec";
    /// <summary>Clé de traduction du nom de la famille de machines du module.</summary>
    internal const string ResourceFamily = "Emulation.Family.Nec";
    /// <summary>Chemin du dossier des Assets externes du module, relatif au dossier racine de l’application.</summary>
    internal const string AssetPathPrefix = "Assets/Emulation/nec";
    /// <summary>Chemin relatif de l’image externe représentant la marque du module.</summary>
    internal const string BrandAssetPath = AssetPathPrefix + "/nec.png";
}
