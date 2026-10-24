namespace GWGUI.Emulation.Interfaces;

/// <summary>Surveille les signatures d’échec annoncées par un cœur, sans lever depuis son callback natif.</summary>
/// <remarks>Les signatures sont fournies par le module et restent stables pendant la session.
/// La capture conserve uniquement le premier message correspondant. Aucun cœur, thread, fichier ou
/// callback natif n’est possédé par ce service ; sa durée de vie est celle de la session qui le conserve.</remarks>
public interface IEmulationCoreDiagnosticMonitor
{
    /// <summary>Premier message correspondant à une signature d’échec, ou null si aucun n’a été reçu.</summary>
    string? Failure { get; }

    /// <summary>Reçoit un message et mémorise le premier échec, sans construire ni lever son exception.</summary>
    /// <param name="message">Texte emprunté provenant du journal du cœur.</param>
    /// <remarks>Les comparaisons ordinales sont exactes. Une collection vide de signatures ne refuse aucun message.
    /// Plusieurs callbacks peuvent appeler cette opération ; un échec déjà retenu ne peut plus être remplacé.</remarks>
    void Capture(string message);

    /// <summary>Lève l’erreur du premier échec à un point où le contrôle est revenu au code géré.</summary>
    /// <remarks>La fabrique fournie par Common détermine son type et son texte. Aucun appel à cette fabrique
    /// n’a lieu pendant Capture. Un appel sans échec capturé retourne normalement.</remarks>
    void ThrowIfFailed();
}
