using GWGUI.Emulation;

namespace GWGUI.Emulation.Philips.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntimeStatus, GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>
{
    /// <summary>Demande de fermeture émise par le cœur et conservée jusqu'à l'arrêt de sa session.</summary>
    /// <remarks>La machine examine ce signal dans sa boucle ; son propriétaire ferme et libère le cœur dans finally.</remarks>
    bool ShutdownRequested { get; }

    Emulator Emulator { get; }
    HostProcessState HostProcessState { get; }
    int? HostProcessId { get; }
    void SaveMediaChanges(EmulationMediaSlot slot);
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
}
