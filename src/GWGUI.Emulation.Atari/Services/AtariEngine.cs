using GWGUI.Emulation;

namespace GWGUI.Emulation.Atari.Services;

public sealed class AtariEngine
{
    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters;

    public AtariEngine()
    {
        var adapters = AtariCoreCatalog.CreateAdapters();
        _adapters = adapters.ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    }

    internal IEmulatedMachine CreateMachine(AtariMachineConfiguration configuration,
        EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        return _adapters.TryGetValue(AtariCoreCatalog.Get(configuration.Core).Id, out var adapter)
            ? adapter.Create(configuration, context)
            : throw new ArgumentOutOfRangeException(nameof(configuration));
    }
}
