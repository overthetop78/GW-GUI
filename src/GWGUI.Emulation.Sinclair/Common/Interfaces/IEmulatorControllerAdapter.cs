namespace GWGUI.Emulation.Sinclair.Common.Interfaces;

/// <summary>Contrat Common unique de description des contrôleurs et de projection des entrées vers un cœur.</summary>
/// <remarks>Chaque opération reçoit la configuration complète de session. Les descriptions conservent les
/// contraintes du modèle, les options du cœur, les ports supplémentaires et les commandes réellement disponibles.
/// Le contrat ne possède aucun périphérique physique, cœur, processus ou ressource graphique.</remarks>
internal interface IEmulatorControllerAdapter : IEmulatorInputMapper
{
    /// <summary>Décrit les actions d’un périphérique sur un port, avec leurs touches émulées éventuelles.</summary>
    /// <param name="configuration">Configuration complète empruntée : modèle, cœur choisi, options et associations.</param>
    /// <param name="port">Indice technique du port, commençant à zéro.</param>
    /// <param name="peripheral">Catégorie de périphérique dont les commandes sont consultées.</param>
    /// <returns>Actions existantes ; les adaptations natives conservent les touches et directions opposées nécessaires.</returns>
    IReadOnlyList<InputActionDefinition> DescribeInput(MachineConfiguration configuration, int port, PeripheralCategory peripheral);

    /// <summary>Consulte les commandes particulières d’un périphérique et d’un port.</summary>
    /// <param name="configuration">Configuration empruntée donnant le modèle et les options du cœur.</param>
    /// <param name="peripheral">Catégorie de périphérique choisie.</param>
    /// <param name="port">Indice technique du port, commençant à zéro.</param>
    /// <returns>Définitions particulières, ou null pour laisser la description matérielle ordinaire.</returns>
    IReadOnlyList<InputBindingDefinition>? GetControllerBindingDefinitions(MachineConfiguration configuration, PeripheralCategory peripheral, int port);

    /// <summary>Calcule les ports disponibles avec les options et extensions de la machine.</summary>
    /// <param name="configuration">Configuration empruntée pouvant activer un multitap ou une extension.</param>
    /// <returns>Nombre de ports, distinct de leur dernier indice ; zéro pour une machine sans port de contrôleur.</returns>
    int GetControllerPortCount(MachineConfiguration configuration);

    /// <summary>Fournit un nom explicite de port lorsque la présentation ordinaire ne suffit pas.</summary>
    /// <param name="configuration">Configuration empruntée déterminant l’organisation des ports.</param>
    /// <param name="port">Indice technique du port, commençant à zéro.</param>
    /// <returns>Nom du port, ou null pour laisser la présentation commune.</returns>
    string? GetControllerPortDisplayName(MachineConfiguration configuration, int port);

    /// <summary>Fournit le numéro présenté lorsque celui-ci diffère du numéro technique du port.</summary>
    /// <param name="configuration">Configuration empruntée déterminant l’organisation des ports.</param>
    /// <param name="port">Indice technique du port, commençant à zéro.</param>
    /// <returns>Numéro explicite, ou null pour employer la numérotation commune.</returns>
    int? GetControllerPortDisplayNumber(MachineConfiguration configuration, int port);

    /// <summary>Décrit les catégories de périphériques disponibles pour la configuration.</summary>
    /// <param name="configuration">Configuration empruntée donnant le modèle, le cœur et ses options.</param>
    /// <returns>Catégories prises en charge dans leur ordre de description.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration);

    /// <summary>Décrit les choix autorisés sur un port de contrôleur particulier.</summary>
    /// <param name="configuration">Configuration complète empruntée.</param>
    /// <param name="port">Indice technique du port, commençant à zéro.</param>
    /// <returns>Catégories disponibles sur ce port, ou liste vide sans port correspondant.</returns>
    IReadOnlyList<PeripheralCategory> GetPeripherals(MachineConfiguration configuration, int port);

    /// <summary>Décrit les périphériques raccordables à un port du bus USB émulé.</summary>
    /// <param name="configuration">Configuration complète du modèle et de son bus USB.</param>
    /// <param name="port">Indice relatif au bus USB, commençant à zéro.</param>
    /// <returns>Choix du port USB, ou liste vide lorsque ce bus ne fournit aucun port correspondant.</returns>
    IReadOnlyList<PeripheralCategory> GetUsbPeripherals(MachineConfiguration configuration, int port);

    /// <summary>Décrit les commandes du clavier principal acceptées par le modèle et le backend.</summary>
    /// <param name="configuration">Configuration complète empruntée, consultée sans la modifier.</param>
    /// <returns>Définitions des associations de clavier ; liste vide lorsque ce clavier n'est pas pris en charge.</returns>
    /// <remarks>Les libellés et identifiants restent ceux du protocole du modèle. Cette consultation ne
    /// capture aucun périphérique physique et ne démarre aucun cœur.</remarks>
    IReadOnlyList<InputBindingDefinition> GetKeyboardDefinitions(MachineConfiguration configuration);

    /// <summary>Adapte les associations préparées par Common aux contraintes du backend.</summary>
    /// <param name="configuration">Configuration empruntée après préparation de ses associations.</param>
    /// <returns>Configuration adaptée ; la même instance lorsque le backend n'a aucune règle supplémentaire.</returns>
    /// <remarks>Les adaptations conservent les autres données de la configuration et ne modifient pas
    /// les collections de l'appelant. L'opération ne possède ni cœur, ni fichier, ni périphérique.</remarks>
    MachineConfiguration ApplyInputConfiguration(MachineConfiguration configuration);
}
