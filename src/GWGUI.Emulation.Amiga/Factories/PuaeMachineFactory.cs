using GWGUI.Emulation;
using GWGUI.Emulation.Amiga.Cores;

namespace GWGUI.Emulation.Amiga;

internal sealed class PuaeMachineFactory : IAmigaMachineFactory
{
    public IEmulatedMachine Create(AmigaMachineConfiguration configuration, AmigaMachineCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var core = new AmigaProcessCore(context.HostExecutablePath, context.CorePath);
        return new AmigaMachine(machineId, configuration.EnsureId(), core,
            Path.Combine(context.SessionsDirectory, machineId.ToString("N")),
            configuration.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
