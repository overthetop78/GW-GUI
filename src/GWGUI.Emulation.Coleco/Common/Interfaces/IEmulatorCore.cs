using GWGUI.Emulation;

namespace GWGUI.Emulation.Coleco.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntimeStatus, GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory, DiskStatus>
{
    Emulator Emulator { get; }
    HostProcessState HostProcessState { get; }
    int? HostProcessId { get; }
    void SaveMediaChanges(EmulationMediaSlot slot);
    bool HasUnsavedMediaChanges(EmulationMediaSlot slot);
}
