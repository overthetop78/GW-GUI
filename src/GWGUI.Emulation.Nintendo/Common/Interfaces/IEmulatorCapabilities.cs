namespace GWGUI.Emulation.Nintendo.Common.Interfaces;

/// <summary>Capacités communes annoncées par chaque adaptateur pour la présentation et l’exécution de ses machines.</summary>
/// <remarks>Les capacités décrivent ce que le backend sait accepter ; elles ne possèdent ni périphérique
/// physique ni session. Le contrat de pointeur tient compte de la configuration, car les ports et les options
/// peuvent activer une souris ou un autre périphérique de pointage. Common utilise les mêmes membres pour
/// tous les backends ; une capacité non prise en charge est annoncée par false.</remarks>
internal interface IEmulatorCapabilities : IEmulatorPointerCapabilities
{
    /// <summary>Indique si l’adaptateur propose les réglages vidéo du contrat Common correspondant.</summary>
    /// <remarks>Cette capacité décrit les éditeurs de configuration, indépendamment de l’émission de frames vidéo.</remarks>
    bool SupportsVideoSettings { get; }

    /// <summary>Indique si un cœur peut rester initialisé sans contenu inséré.</summary>
    /// <remarks>La fabrique transmet cette capacité à sa machine. Lors d’une éjection nécessitant un
    /// redémarrage, elle détermine si le cœur peut être recréé sans média ou doit attendre l’insertion suivante.</remarks>
    bool SupportsNoContent { get; }
}
