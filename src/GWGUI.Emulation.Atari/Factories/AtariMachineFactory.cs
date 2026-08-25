using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Factories;

internal abstract class AtariMachineFactory(AtariEmulator emulator) : IAtariMachineFactory
{
    public AtariEmulator Emulator { get; } = emulator;

    public IEmulatedMachine Create(AtariMachineConfiguration configuration, AtariMachineCreationContext context)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();
        if (configuration.Core != Emulator)
            throw new ArgumentException(nameof(configuration));
        var machineId = Guid.NewGuid();
        var core = new AtariProcessCore(context.HostExecutablePath, context.CorePath, Emulator);
        return new AtariMachine(machineId, configuration, core,
            Path.Combine(context.SessionsDirectory, machineId.ToString(AtariEngineConstants.IdentifierFormat)),
            audioOutputFactory: configuration.AudioEnabled ? context.AudioOutputFactory : null,
            saveDirectory: context.SaveDirectoryResolver?.Invoke(configuration));
    }
}
