using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Interfaces;

internal interface IAmigaMachineFactory
{
    IEmulatedMachine Create(AmigaMachineConfiguration configuration, AmigaMachineCreationContext context);
}
