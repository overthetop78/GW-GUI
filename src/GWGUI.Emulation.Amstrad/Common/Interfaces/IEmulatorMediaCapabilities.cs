namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

/// <summary>Description des lecteurs logiques proposés par un adaptateur pour une configuration.</summary>
/// <remarks>Common utilise ces descriptions pour présenter et contrôler les montages des médias.
/// Les identités des emplacements, les types, les extensions et les capacités restent ceux du backend.
/// Ce contrat ne possède ni lecteur physique, ni fichier de contenu, ni session de cœur.</remarks>
internal interface IEmulatorMediaCapabilities
{
    /// <summary>Décrit les lecteurs disponibles pour le modèle et les options de la configuration.</summary>
    /// <param name="configuration">Configuration empruntée à consulter sans la remplacer.</param>
    /// <returns>Liste en lecture seule de descriptions de lecteurs, avec leurs emplacements et leurs contraintes.</returns>
    /// <remarks>La liste décrit les lecteurs ; elle reste distincte des médias actuellement montés,
    /// des emplacements configurés et des réglages supplémentaires des périphériques. L’appelant
    /// conserve les règles de projection et de validation du stockage qui accompagnent ces descriptions.
    /// Aucun transfert de propriété des fichiers référencés n’est effectué par cette opération.</remarks>
    IReadOnlyList<EmulationMediaDevice> DescribeMediaDevices(MachineConfiguration configuration);
}
