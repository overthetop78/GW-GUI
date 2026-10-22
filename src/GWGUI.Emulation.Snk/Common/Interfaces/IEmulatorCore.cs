using GWGUI.Emulation;

namespace GWGUI.Emulation.Snk.Common.Interfaces;

internal interface IEmulatorCore : IEmulatorAudioBufferFeedback, GWGUI.Emulation.Interfaces.IEmulationCoreRuntimeStatus, GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory, DiskStatus>
{
    Emulator Emulator { get; }
    HostProcessState HostProcessState { get; }
    int? HostProcessId { get; }
}
