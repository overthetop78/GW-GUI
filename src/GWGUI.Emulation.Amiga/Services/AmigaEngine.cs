using GWGUI.Emulation;
namespace GWGUI.Emulation.Amiga.Services;

public sealed class AmigaEngine
{
    private readonly IReadOnlyDictionary<string, IEmulatorAdapter> _adapters;

    public AmigaEngine()
    {
        var adapters = AmigaCoreCatalog.CreateAdapters();
        _adapters = adapters.ToDictionary(adapter => adapter.EmulatorId, StringComparer.Ordinal);
    }

    internal IEmulatedMachine CreateMachine(AmigaMachineConfiguration configuration,
        EmulatorCreationContext context)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(context);
        return _adapters[AmigaEmulationModuleConstants.Puae].Create(configuration, context);
    }
}
