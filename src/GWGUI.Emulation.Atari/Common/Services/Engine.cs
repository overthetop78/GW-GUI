using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Common.Services;

public sealed class Engine
{
    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters;

    public Engine()
    {
        var adapters = EmulatorCatalog.CreateAdapters();
        _adapters = adapters.ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    }

    internal IEmulatedMachine CreateMachine(MachineConfiguration configuration,
        EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        return _adapters.TryGetValue(EmulatorCatalog.Get(configuration.Core).Id, out var adapter)
            ? adapter.Create(configuration, context)
            : throw new ArgumentOutOfRangeException(nameof(configuration));
    }

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) =>
        _adapters[EmulatorCatalog.Get(configuration.Core).Id];
}
