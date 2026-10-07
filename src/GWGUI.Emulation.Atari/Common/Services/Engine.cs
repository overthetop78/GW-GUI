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
    internal IEmulatorAdapter Adapter(Emulator emulator) =>
        Adapter(EmulatorCatalog.Get(emulator).Id);

    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter)
        ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId));

    internal IReadOnlyList<IEmulatorAdapter> Adapters(string machineId) => _adapters.Values
        .Where(adapter => adapter.Definition.MachineIds.Contains(machineId))
        .OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();

    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values)
            if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.NativeOperationSuccess;
        return false;
    }
}
