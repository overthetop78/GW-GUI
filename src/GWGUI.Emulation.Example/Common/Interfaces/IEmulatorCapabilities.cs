using GWGUI.Emulation.Example.Common.Machines.Common.Contracts;

namespace GWGUI.Emulation.Example.Common.Interfaces;

/// <summary>Capacités communes annoncées par chaque adaptateur pour la présentation et l’exécution de ses machines.</summary>
/// <remarks>Les capacités décrivent ce que le backend sait accepter ; elles ne possèdent ni périphérique
/// physique ni session. Le contrat de pointeur tient compte de la configuration, car les ports et les options
/// peuvent activer une souris ou un autre périphérique de pointage. Common utilise les mêmes membres pour
/// tous les backends ; une capacité non prise en charge est annoncée par false.</remarks>
internal interface IEmulatorCapabilities
{
    /// <summary>Indique si la configuration dispose d’un pointeur émulé utilisable par le backend.</summary>
    /// <param name="configuration">Modèle, options et associations de ports dont la prise en charge doit être résolue.</param>
    /// <returns>True pour une souris ou une autre entrée de pointage disponible ; false sans prise en charge.</returns>
    /// <remarks>Ce membre réunit la capacité de souris et celle de réglages du pointeur. Le backend inclut
    /// les périphériques USB acceptés lorsqu’ils sont configurés. La capture physique du pointeur et son
    /// activation au clic restent décidées par la machine à partir de cette capacité et des choix de l’utilisateur.</remarks>
    bool SupportsPointer(MachineConfiguration configuration);

    /// <summary>Indique si l’adaptateur propose les réglages vidéo du contrat Common correspondant.</summary>
    /// <remarks>Cette capacité décrit les éditeurs de configuration, indépendamment de l’émission de frames vidéo.</remarks>
    bool SupportsVideoSettings { get; }

    /// <summary>Indique si un cœur peut rester initialisé sans contenu inséré.</summary>
    /// <remarks>La fabrique transmet cette capacité à sa machine. Lors d’une éjection nécessitant un
    /// redémarrage, elle détermine si le cœur peut être recréé sans média ou doit attendre l’insertion suivante.</remarks>
    bool SupportsNoContent { get; }
}
