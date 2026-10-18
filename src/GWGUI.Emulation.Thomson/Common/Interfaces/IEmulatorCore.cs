using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Thomson.Common.Contracts;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Contracts;
using GWGUI.Emulation.Thomson.Common.Machines.Common.Enums;
using GWGUI.Emulation;

namespace GWGUI.Emulation.Thomson.Common.Interfaces;

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
}
