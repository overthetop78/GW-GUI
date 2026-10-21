using GWGUI.Emulation;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Contrat de cœur utilisé par la machine Common pour exécuter et commander un backend.</summary>
/// <remarks>Les contrats hérités portent l’initialisation, les entrées, le calcul des frames, les sorties,
/// les options, les états et les commandes de médias. Common sérialise les accès au cœur pendant l’exécution.
/// La machine qui possède le cœur arrête sa boucle puis le détruit dans finally ; les fichiers de médias reçus
/// restent possédés par leurs appelants. L’hébergement peut être local ou confié à un processus séparé.</remarks>
internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntimeStatus, GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory, DiskStatus>
{
    /// <summary>Identité du backend utilisé par ce cœur, distincte du modèle matériel émulé.</summary>
    Emulator Emulator { get; }
    /// <summary>État du tampon de sortie audio de l’hôte, transmis au cœur pour son suivi de remplissage.</summary>
    /// <remarks>Null indique qu’aucun état de sortie n’est disponible. Le contrat commun exprime la fréquence en Hz
    /// et les capacités en frames audio ; il reste une description empruntée, sans propriété de la sortie physique.</remarks>
    AudioOutputBufferStatus? OutputAudioStatus { get; set; }
    /// <summary>Latence audio minimale demandée par le cœur, exprimée en millisecondes.</summary>
    /// <remarks>Le propriétaire de la sortie utilise cette demande pour adapter son tampon ; elle ne remplace pas la fréquence d’échantillonnage.</remarks>
    int MinimumAudioLatencyMilliseconds { get; }
    /// <summary>Indique que le cœur demande la fermeture de sa session.</summary>
    /// <remarks>La boucle du propriétaire doit examiner ce signal et effectuer son arrêt et sa destruction habituels.</remarks>
    bool ShutdownRequested { get; }
    /// <summary>Retours de périphériques émulés disponibles pour les associations de sortie de la machine.</summary>
    /// <remarks>Une liste vide décrit l’absence d’effets actifs. Le consommateur contrôle la force et les associations
    /// avant de transmettre une commande à un périphérique de l’hôte.</remarks>
    IReadOnlyList<ControllerFeedbackState> ControllerFeedback { get; }
    /// <summary>État du processus d’hébergement, distinct de l’état actif ou en pause de la machine.</summary>
    HostProcessState HostProcessState { get; }
    /// <summary>Identifiant du processus d’hôte lorsqu’il existe ; null en l’absence d’un processus à exposer.</summary>
    int? HostProcessId { get; }
    /// <summary>Demande la réinitialisation logicielle propre au cœur.</summary>
    /// <remarks>Cette opération est distincte de HardReset ; le backend peut utiliser sa commande native ou la combinaison
    /// de touches déclarée par son adaptateur, avec relâchement des entrées à la fin de l’opération.</remarks>
    void SoftReset();
    /// <summary>Demande l’enregistrement explicite des changements du média monté dans un lecteur.</summary>
    /// <param name="slot">Lecteur logique dont le backend doit enregistrer les changements pris en charge.</param>
    /// <remarks>Common appelle cette opération dans le même ordre sérialisé que les autres commandes du cœur.</remarks>
    void SaveMediaChanges(EmulationMediaSlot slot);
    /// <summary>Demande si le média d’un lecteur possède des modifications non encore enregistrées.</summary>
    /// <param name="slot">Lecteur logique à interroger.</param>
    /// <returns>True lorsque des changements restent à enregistrer selon le backend.</returns>
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
}

