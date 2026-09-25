using GWGUI.Emulation;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Factories;

internal sealed class PuaeMachineFactory : IEmulatorAdapter
{
    public string EmulatorId => AmigaEmulationModuleConstants.Puae;

    public AmigaMachine Create(AmigaMachineConfiguration configuration,
        EmulatorCreationContext context)
    {
        var machineId = Guid.NewGuid();
        var core = new AmigaProcessCore(context.HostExecutablePath, context.CorePath);
        return new AmigaMachine(machineId, configuration.EnsureId(), core,
            Path.Combine(context.SessionsDirectory, machineId.ToString(PuaeMachineFactoryConstants.N)),
            configuration.AudioEnabled ? context.AudioOutputFactory?.Invoke() : null,
            context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
