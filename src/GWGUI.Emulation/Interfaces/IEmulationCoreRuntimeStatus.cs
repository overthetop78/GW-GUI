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
}
