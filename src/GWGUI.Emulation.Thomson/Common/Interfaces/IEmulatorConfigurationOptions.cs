namespace GWGUI.Emulation.Thomson.Common.Interfaces;

/// <summary>Préparation des options propres au cœur à partir de la configuration complète.</summary>
/// <remarks>Common appelle ces opérations sans ouvrir de cœur. La configuration et les services qu’elle
/// décrit restent sous la responsabilité de leur propriétaire. L’initialisation complète le dictionnaire
/// de préparation de l’appelant ; elle reste distincte de la validation et de l’application des réglages.</remarks>
internal interface IEmulatorConfigurationOptions
{
    /// <summary>Décrit les valeurs initiales propres au cœur pour cette configuration.</summary>
    /// <param name="configuration">Données empruntées du modèle, du cœur, des contenus et des options courantes.</param>
    /// <returns>Valeurs initiales à ajouter aux options manquantes ; Common utilise une collection vide lorsque le cœur n’en déclare aucune.</returns>
    IReadOnlyDictionary<string, string> GetDefaultOptions(MachineConfiguration configuration);

    /// <summary>Complète les options de préparation selon les données de configuration.</summary>
    /// <param name="configuration">Configuration empruntée, disponible avant validation et démarrage du cœur.</param>
    /// <param name="options">Dictionnaire modifiable appartenant à l’appelant.</param>
    /// <remarks>Le comportement Common ajoute les valeurs initiales sans remplacer les choix existants.
    /// Un cœur peut conserver ici ses règles de préparation dépendant des firmwares et des médias.</remarks>
    void InitializeOptions(MachineConfiguration configuration, IDictionary<string, string> options);

    /// <summary>Contrôle les valeurs et combinaisons d’options natives.</summary>
    /// <param name="configuration">Configuration empruntée à vérifier, conservée immuable.</param>
    /// <remarks>Common réutilise le contrat de validation existant ; les cœurs disposant d’un contrôle
    /// spécifique des options l’implémentent directement. Les erreurs suivent les diagnostics habituels.</remarks>
    void ValidateOptions(MachineConfiguration configuration);

    /// <summary>Résout la variante matérielle déterminée par le modèle et ses options.</summary>
    /// <param name="configuration">Configuration empruntée contenant le modèle de départ et les valeurs proposées.</param>
    /// <returns>Modèle effectif ; Common conserve le modèle reçu lorsqu’aucune adaptation native n’est définie.</returns>
    MachineModel ResolveModel(MachineConfiguration configuration);

    /// <summary>Normalise les valeurs techniques selon les conventions du cœur.</summary>
    /// <param name="configuration">Configuration empruntée contenant les options à préparer.</param>
    /// <returns>Options normalisées ; Common conserve la collection reçue, ou retourne une collection vide si elle est absente.</returns>
    /// <remarks>Le retour ne transfère aucune propriété de la configuration. Une normalisation qui change
    /// les valeurs construit son propre dictionnaire et laisse intactes les données de l’appelant.</remarks>
    IReadOnlyDictionary<string, string> NormalizeOptions(MachineConfiguration configuration);
}
