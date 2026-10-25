namespace GWGUI.Emulation.Enums;

/// <summary>État de l’hébergement du cœur, distinct du cycle de vie de la machine émulée.</summary>
/// <remarks>Le processus d’hôte peut rester actif lorsque la machine est en pause. Le SDK expose la même
/// identité de type à tous les modules ; aucune définition parallèle par famille n’est nécessaire.</remarks>
public enum HostProcessState
{
    /// <summary>Le cœur utilise le processus courant ; aucun processus enfant n’est possédé.</summary>
    InProcess = 0,
    /// <summary>Aucun processus d’hôte n’a encore démarré.</summary>
    NotStarted = 1,
    /// <summary>Le processus d’hôte est actif.</summary>
    Running = 2,
    /// <summary>La fin du processus d’hôte a été observée, même après la libération de son objet géré.</summary>
    Exited = 3,
    /// <summary>Le démarrage ou la communication avec l’hôte a échoué.</summary>
    Faulted = 4
}