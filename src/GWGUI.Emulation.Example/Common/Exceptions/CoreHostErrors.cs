namespace GWGUI.Emulation.Example.Common.Exceptions;

/// <summary>Messages utilisés lors du démarrage et des échanges avec un hôte de cœur.</summary>
/// <remarks>Les propriétés résolvent le texte dans la culture d’interface au moment de l’accès.
/// Plusieurs situations utilisent le même diagnostic général ; les valeurs particulières sont fournies par
/// le contexte ou par les arguments de formatage du code appelant. Aucun texte traduit n’est conservé ici.</remarks>
internal static class CoreHostErrors
{
    /// <summary>Message employé lorsque la configuration reçue par l’hôte ne convient pas.</summary>
    internal static string InvalidConfiguration => HostFailure();
    /// <summary>Message employé lorsqu’une opération exige une initialisation qui n’a pas eu lieu.</summary>
    internal static string NotInitialized => HostFailure();
    /// <summary>Message employé lorsqu’une seconde initialisation est refusée.</summary>
    internal static string AlreadyInitialized => HostFailure();
    /// <summary>Message spécifique à l’absence de l’exécutable utilisé pour lancer l’hôte.</summary>
    internal static string ExecutableMissing => ExceptionText.Get("Emulation.Example.Error.HostExecutableMissing");
    /// <summary>Message employé lorsque le processus d’hôte ne peut pas démarrer.</summary>
    internal static string ProcessStartFailed => HostFailure();
    /// <summary>Message employé lorsque le processus ou son transport n’est plus disponible.</summary>
    internal static string ProcessUnavailable => HostFailure();
    /// <summary>Message employé lorsque le processus n’est pas encore prêt à recevoir l’opération.</summary>
    internal static string ProcessNotInitialized => HostFailure();
    /// <summary>Message employé lorsque le bloc de vidéo partagée ne peut pas être utilisé.</summary>
    internal static string SharedVideoUnavailable => HostFailure();
    /// <summary>Message employé lorsqu’aucune réponse exploitable ne peut être obtenue.</summary>
    internal static string ResponseUnavailable => HostFailure();
    /// <summary>Message employé lorsque le délai de réception d’une réponse est dépassé.</summary>
    internal static string ResponseTimeout => HostFailure();
    /// <summary>Message employé lorsqu’une requête de l’hôte a été annulée.</summary>
    internal static string RequestCancelled => HostFailure();
    /// <summary>Message employé après une rupture ou un échec de communication.</summary>
    internal static string CommunicationFailed => HostFailure();
    /// <summary>Texte utilisé pour signaler une différence de version de protocole.</summary>
    internal static string ProtocolVersionMismatchFormat => HostFailure();
    /// <summary>Texte utilisé pour signaler un code de commande inconnu.</summary>
    internal static string UnknownCommandFormat => HostFailure();
    /// <summary>Texte utilisé pour signaler une taille de réponse invalide.</summary>
    internal static string InvalidResponseLengthFormat => HostFailure();
    /// <summary>Texte de détail ajouté au diagnostic pour restituer la fin du processus.</summary>
    internal static string ProcessExitSuffixFormat => ExceptionText.Get("Emulation.Example.Error.Details");

    /// <summary>Résout le diagnostic général de protocole dans la culture d’interface courante.</summary>
    /// <returns>Texte traduit, ou clé de ressource lorsque le catalogue ne fournit aucune valeur.</returns>
    private static string HostFailure() => ExceptionText.Get("Emulation.Example.Error.HostProtocolFailure");
}
