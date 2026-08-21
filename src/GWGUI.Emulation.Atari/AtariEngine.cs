using GWGUI.Emulation;
using System.Runtime.Versioning;

namespace GWGUI.Emulation.Atari;

[SupportedOSPlatform("windows")]
public sealed class AtariEngine
{
    private readonly IReadOnlyDictionary<AtariEmulator, IAtariMachineFactory> _factories;

    public AtariEngine()
    {
        IAtariMachineFactory[] factories =
        [
            new HatariMachineFactory(), new Atari800MachineFactory(), new StellaMachineFactory(),
            new ProSystemMachineFactory(), new BeetleLynxMachineFactory(), new VirtualJaguarMachineFactory()
        ];
        _factories = factories.ToDictionary(factory => factory.Emulator);
    }

    internal IEmulatedMachine CreateMachine(AtariMachineConfiguration configuration,
        AtariMachineCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        return _factories.TryGetValue(configuration.Core, out var factory)
            ? factory.Create(configuration, context)
            : throw new ArgumentOutOfRangeException(nameof(configuration));
    }
}
