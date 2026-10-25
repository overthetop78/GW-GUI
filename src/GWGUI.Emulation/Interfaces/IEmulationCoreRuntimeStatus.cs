namespace GWGUI.Emulation.Interfaces;

/// <summary>Capacités et compteurs réels du cœur, indépendants de la présentation de l’application.</summary>
public interface IEmulationCoreRuntimeStatus
{
    /// <summary>Indique si le cœur courant peut produire et restaurer un état.</summary>
    bool SupportsSaveStates { get; }
    /// <summary>Standard vidéo détecté, ou null lorsqu’il n’est pas connu.</summary>
    EmulationVideoStandard? Region { get; }
    /// <summary>Nombre de frames audio actuellement en attente.</summary>
    int BufferedAudioFrames { get; }
    /// <summary>Nombre de débordements du tampon audio.</summary>
    long AudioOverrunCount { get; }
    /// <summary>Nombre de manques de données dans le tampon audio.</summary>
    long AudioUnderrunCount { get; }
    /// <summary>État du processus qui héberge le cœur, distinct de la pause ou de l’arrêt de la machine.</summary>
    HostProcessState HostProcessState { get; }
    /// <summary>Identifiant de l’hôte démarré, conservé après sa fermeture ; null sans processus enfant.</summary>
    /// <remarks>Cette valeur ne possède aucun processus ni handle. Le propriétaire ferme son hôte dans finally.</remarks>
    int? HostProcessId { get; }
}
