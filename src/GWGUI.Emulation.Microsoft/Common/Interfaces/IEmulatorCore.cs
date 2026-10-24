namespace GWGUI.Emulation.Microsoft.Common.Interfaces;

internal interface IEmulatorCore : GWGUI.Emulation.Interfaces.IEmulationCoreRuntime<MachineConfiguration, CoreOption>, GWGUI.Emulation.Interfaces.IEmulationCoreCommands<MediaConfiguration, PeripheralCategory>
{

    int DiskCount { get; }
    int CurrentDiskIndex { get; }
}
