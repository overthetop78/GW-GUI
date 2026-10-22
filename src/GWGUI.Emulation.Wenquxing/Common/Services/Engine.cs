using GWGUI.Emulation.Wenquxing.Common.Dictionaries;

namespace GWGUI.Emulation.Wenquxing.Common.Services;

public sealed class Engine
{
    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters = EmulatorCatalog.CreateAdapters().ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    internal IEmulatedMachine CreateMachine(MachineConfiguration configuration, EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ConfigurationFunctions.Validate(configuration);
        return Adapter(configuration).Create(configuration, context);
    }
    /// <summary>Retrouve l’adaptateur sélectionné sans charger de cœur ; null conserve le parcours sans sélection.</summary>
    internal IEmulatorAdapter? SelectedAdapter(MachineConfiguration configuration) => _adapters.Values.FirstOrDefault(adapter => adapter.EmulatorKey == configuration.Core.ToString());

    internal IEmulatorAdapter Adapter(MachineConfiguration configuration) => Adapter(configuration.Core);
    internal IEmulatorAdapter Adapter(Emulator emulator) => _adapters.Values.SingleOrDefault(adapter => string.Equals(adapter.EmulatorKey, emulator.ToString(), StringComparison.Ordinal)) ?? throw new ArgumentOutOfRangeException(nameof(emulator));
    internal IEmulatorAdapter Adapter(string emulatorId) => _adapters.TryGetValue(emulatorId, out var adapter) ? adapter : throw new ArgumentOutOfRangeException(nameof(emulatorId));
    internal IReadOnlyList<IEmulatorAdapter> Adapters(string machineId) => _adapters.Values.Where(adapter => adapter.Definition.MachineIds.Contains(machineId)).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();
    internal bool TryHandleHostCommand(IReadOnlyList<string> arguments, out int exitCode)
    {
        foreach (var adapter in _adapters.Values) if (adapter.TryHandleHostCommand(arguments, out exitCode)) return true;
        exitCode = CoreHostConstants.NativeOperationSuccess;
        return false;
    }
}
