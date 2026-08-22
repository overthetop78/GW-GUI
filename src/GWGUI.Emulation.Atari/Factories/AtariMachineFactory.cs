using GWGUI.Emulation;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Emulation.Atari;

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

internal sealed class HatariMachineFactory() : AtariMachineFactory(AtariEmulator.Hatari);
internal sealed class Atari800MachineFactory() : AtariMachineFactory(AtariEmulator.Atari800);
internal sealed class StellaMachineFactory() : AtariMachineFactory(AtariEmulator.Stella);
internal sealed class ProSystemMachineFactory() : AtariMachineFactory(AtariEmulator.ProSystem);
internal sealed class BeetleLynxMachineFactory() : AtariMachineFactory(AtariEmulator.BeetleLynx);
internal sealed class VirtualJaguarMachineFactory() : AtariMachineFactory(AtariEmulator.VirtualJaguar);
