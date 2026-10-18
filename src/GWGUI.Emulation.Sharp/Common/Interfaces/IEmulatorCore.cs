using GWGUI.Emulation;

namespace GWGUI.Emulation.Sharp.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory, DiskStatus>
{
    Emulator Emulator { get; }
    bool SupportsSaveStates { get; }
    EmulationVideoStandard? Region { get; }
    int BufferedAudioFrames { get; }
    long AudioOverrunCount { get; }
    long AudioUnderrunCount { get; }
    HostProcessState HostProcessState { get; }
    int? HostProcessId { get; }
    void SaveMediaChanges(EmulationMediaSlot slot);
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
}
