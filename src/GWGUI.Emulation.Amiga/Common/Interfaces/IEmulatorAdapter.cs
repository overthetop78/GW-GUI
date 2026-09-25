namespace GWGUI.Emulation.Amiga.Common.Interfaces;

internal interface IEmulatorAdapter
{
    string EmulatorId { get; }
    AmigaMachine Create(AmigaMachineConfiguration configuration, EmulatorCreationContext context);
}
