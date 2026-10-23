namespace GWGUI.Emulation.Acorn.Common.Interfaces;

/// <summary>Préparation des options et variantes à partir de la configuration complète.</summary>
internal interface IEmulatorConfigurationOptions
{
    /// <summary>Décrit les valeurs initiales du cœur pour la configuration empruntée.</summary>
    IReadOnlyDictionary<string, string> GetDefaultOptions(MachineConfiguration configuration);
    /// <summary>Contrôle les valeurs natives sans modifier les données reçues.</summary>
    void ValidateOptions(MachineConfiguration configuration);
    /// <summary>Résout la variante matérielle depuis le modèle et les options reçus.</summary>
    MachineModel ResolveModel(MachineConfiguration configuration) => configuration.Model;
    /// <summary>Retourne les options normalisées ; le repli conserve la même collection empruntée.</summary>
    IReadOnlyDictionary<string, string> NormalizeOptions(MachineConfiguration configuration) => configuration.Options;
}
