namespace GWGUI.Emulation.Microsoft.Common.Interfaces;

internal interface IEmulatorCore : IEmulatorAudioBufferFeedback, IEmulationCoreRuntimeStatus, GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreStateCommands
{
    /// <summary>Demande de fermeture émise par le cœur et conservée jusqu'à l'arrêt de sa session.</summary>
    /// <remarks>La machine examine ce signal dans sa boucle ; son propriétaire ferme et libère le cœur dans finally.</remarks>
    bool ShutdownRequested { get; }

    int DiskCount { get; }
    int CurrentDiskIndex { get; }
    void InsertMedia(string path);
    void EjectMedia();
}
