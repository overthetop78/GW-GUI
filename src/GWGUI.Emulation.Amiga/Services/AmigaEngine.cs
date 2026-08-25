using GWGUI.Emulation;
namespace GWGUI.Emulation.Amiga.Services;

public sealed class AmigaEngine
{
    private readonly IAmigaMachineFactory _puaeFactory = new PuaeMachineFactory();

    internal IEmulatedMachine CreateMachine(AmigaMachineConfiguration configuration,
        AmigaMachineCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        return _puaeFactory.Create(configuration, context);
    }
}
