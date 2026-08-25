using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Factories;

internal sealed class PuaeMachineFactory : IAmigaMachineFactory
{
    public IEmulatedMachine Create(AmigaMachineConfiguration configuration, AmigaMachineCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var core = new AmigaProcessCore(context.HostExecutablePath, context.CorePath);
        return new AmigaMachine(machineId, configuration.EnsureId(), core,
            Path.Combine(context.SessionsDirectory, machineId.ToString(PuaeMachineFactoryConstants.N)),
            configuration.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
