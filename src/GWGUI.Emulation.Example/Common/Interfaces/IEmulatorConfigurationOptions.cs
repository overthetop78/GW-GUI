namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Adaptation des options propres à un backend lors de la préparation d’une configuration.</summary>
/// <remarks>Le module utilise ce contrat pour obtenir les valeurs initiales, contrôler les options et résoudre
/// un modèle dépendant de ces options. Les méthodes par défaut conservent les valeurs et références reçues.</remarks>
internal interface IEmulatorConfigurationOptions
{
    /// <summary>Décrit les options initiales du backend pour le modèle demandé.</summary>
    /// <param name="model">Modèle avant application des éventuelles options de variante matérielle.</param>
    /// <returns>Dictionnaire de clés et valeurs techniques utilisé pour préparer une nouvelle configuration.</returns>
    IReadOnlyDictionary<string, string> GetDefaultOptions(MachineModel model);
    /// <summary>Complète les options préparées avec les valeurs dépendant de la configuration complète.</summary>
    /// <param name="configuration">Configuration empruntée, incluant médias et firmwares.</param>
    /// <param name="options">Dictionnaire de préparation modifiable appartenant à l’appelant.</param>
    /// <remarks>Cette étape complète les valeurs par modèle ; elle ne remplace ni la validation ni l’application des réglages utilisateur.</remarks>
    void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options);
    /// <summary>Vérifie les valeurs et combinaisons d’options d’une configuration.</summary>
    /// <param name="configuration">Configuration empruntée ; la validation ne la remplace pas.</param>
    /// <remarks>L’implémentation signale les options refusées par les diagnostics du module.</remarks>
    void ValidateOptions(MachineConfiguration configuration);
    /// <summary>Résout le modèle matériel effectivement sélectionné par les options.</summary>
    /// <param name="model">Modèle de départ de la configuration.</param>
    /// <param name="options">Options empruntées, stables pendant l’appel.</param>
    /// <returns>Modèle effectif ; l’implémentation par défaut retourne le modèle reçu.</returns>
    MachineModel ResolveModel(MachineModel model, IReadOnlyDictionary<string, string> options) => model;
    /// <summary>Normalise les options selon les conventions du backend pour le modèle.</summary>
    /// <param name="model">Modèle auquel les options s’appliquent.</param>
    /// <param name="options">Dictionnaire emprunté ; aucune modification ne doit être supposée par l’appelant.</param>
    /// <returns>Options normalisées ; la méthode par défaut retourne la même instance sans copie.</returns>
    IReadOnlyDictionary<string, string> NormalizeOptions(MachineModel model, IReadOnlyDictionary<string, string> options) => options;
}
