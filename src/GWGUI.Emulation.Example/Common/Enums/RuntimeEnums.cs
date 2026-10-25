namespace GWGUI.Emulation.Example.Common.Enums;

/// <summary>Domaine fonctionnel d’un diagnostic, indépendant de la langue du message.</summary>
public enum ErrorCategory
{
    /// <summary>Résolution, chargement ou validation du cœur d’émulation.</summary>
    Core,
    /// <summary>Présence et compatibilité des firmwares de la machine.</summary>
    Firmware,
    /// <summary>Présence, format et montage du contenu à émuler.</summary>
    Content,
    /// <summary>Valeurs et combinaisons des réglages de configuration.</summary>
    Option,
    /// <summary>Démarrage et communication du processus qui héberge le cœur.</summary>
    Host,
    /// <summary>Format, intégrité et compatibilité des sauvegardes d’état.</summary>
    State
}

/// <summary>Motif technique stable précisant la catégorie d’une erreur.</summary>
/// <remarks>Les récepteurs reconnaissent Category et Code sans analyser le message traduit.
/// Le contexte de l’exception fournit les valeurs propres à l’échec, comme le chemin ou l’identité attendue.</remarks>
public enum ErrorCode
{
    /// <summary>Le cœur demandé est absent ou son chemin ne peut pas être résolu.</summary>
    CoreNotFound,
    /// <summary>Le cœur trouvé ne satisfait pas le contrat de chargement ou l’identité attendue.</summary>
    CoreRejected,
    /// <summary>Un firmware obligatoire n’a pas été fourni.</summary>
    FirmwareMissing,
    /// <summary>Le firmware fourni ne correspond pas au modèle ou à l’emplacement demandé.</summary>
    FirmwareInvalid,
    /// <summary>Le fichier ou dossier du contenu demandé est absent.</summary>
    ContentNotFound,
    /// <summary>Le démarrage exige la sélection d’un contenu.</summary>
    ContentRequired,
    /// <summary>Le contenu n’est pas accepté par le cœur ou le lecteur choisi.</summary>
    ContentUnsupported,
    /// <summary>Un réglage ou une association de périphérique n’a pas une valeur admise.</summary>
    OptionInvalid,
    /// <summary>Une requête ou une réponse d’hôte ne peut pas être échangée ou interprétée correctement.</summary>
    HostProtocolFailure,
    /// <summary>Le document ou la charge utile d’une sauvegarde d’état est invalide.</summary>
    StateInvalid,
    /// <summary>La sauvegarde est valide mais son identité ne correspond pas à la machine courante.</summary>
    StateIncompatible
}

