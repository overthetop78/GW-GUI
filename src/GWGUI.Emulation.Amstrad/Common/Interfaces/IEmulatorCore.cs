using GWGUI.Emulation;

namespace GWGUI.Emulation.Amstrad.Common.Interfaces;

internal interface IEmulatorCore : IEmulationCoreRuntimeStatus, GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCorePathMediaCommands
{
    /// <summary>Demande de fermeture émise par le cœur et conservée jusqu'à l'arrêt de sa session.</summary>
    /// <remarks>La machine examine ce signal dans sa boucle ; son propriétaire ferme et libère le cœur dans finally.</remarks>
    bool ShutdownRequested { get; }

    bool SupportsPointerCapture => true;
}
