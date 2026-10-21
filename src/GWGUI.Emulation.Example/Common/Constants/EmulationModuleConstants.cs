namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Identité du module, noms de ressources embarquées et répertoires persistants.</summary>
/// <remarks>ModuleId est une identité technique stable. ResourceFamily est une clé à résoudre par la localisation.
/// Les noms de ressources embarquées doivent correspondre aux noms logiques présents dans l’assembly.
/// Les noms de dossiers sont relatifs à la racine de données du module fournie par l’application.</remarks>
internal static class EmulationModuleConstants
{
    /// <summary>Identifiant technique utilisé par le registre et les configurations du module.</summary>
    internal const string ModuleId = "example";
    /// <summary>Clé de localisation du nom de famille présenté dans l’application.</summary>
    internal const string ResourceFamily = "Emulation.Family.Example";
    /// <summary>Préfixe commun des noms logiques des images et autres ressources embarquées du module.</summary>
    internal const string AssetResourcePrefix = "GWGUI.Emulation.Example.Assets";
    /// <summary>Nom logique complet de l’image de modèle recherchée dans les ressources de l’assembly.</summary>
    internal const string ExampleMachineAImageResourceName = AssetResourcePrefix + ".Machines.ExampleMachineA.png";
    /// <summary>Préfixe des catalogues de traduction utilisé par le service de localisation du module.</summary>
    internal const string LocalizationResourceName = "GWGUI.Emulation.Example.Resources.Emulation";
    /// <summary>Dossier des configurations de machines enregistrées sous la racine du module.</summary>
    internal const string ConfigurationsDirectoryName = "Configurations";
    /// <summary>Dossier des installations de cœurs gérées par le module.</summary>
    internal const string CoreDirectoryName = "Core";
}

