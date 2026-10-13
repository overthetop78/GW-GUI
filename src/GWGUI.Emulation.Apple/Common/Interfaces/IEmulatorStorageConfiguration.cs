namespace GWGUI.Emulation.Apple.Common.Interfaces;

internal interface IEmulatorStorageConfiguration
{
    MachineConfiguration ApplyStorageConfiguration(MachineConfiguration configuration, IReadOnlyList<EmulationMediaSlot> slots);
}
