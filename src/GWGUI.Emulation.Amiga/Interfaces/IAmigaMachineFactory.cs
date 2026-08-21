using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga;

internal interface IAmigaMachineFactory
{
    IEmulatedMachine Create(AmigaMachineConfiguration configuration, AmigaMachineCreationContext context);
}
