namespace GWGUI.Emulation.Atari.Common.Interfaces;

internal interface IEmulatorAdapter
{
    string EmulatorId { get; }
    AtariMachine Create(AtariMachineConfiguration configuration, EmulatorCreationContext context);
}
