using System.Reflection;

namespace GWGUI.Emulation.Services;

/// <summary>Adaptation interne des algorithmes de découverte existants au contrat public du SDK.</summary>
internal sealed class EmulationAdapterService : IEmulationAdapters
{
    public string DisplayResourceKey(IReadOnlyList<EmulationMachineDefinition> models, IEmulationConfiguration configuration) => EmulationMachineModelFunctions.DisplayResourceKey(models, configuration);
    public TModel Parse<TModel>(string id) where TModel : struct, Enum => EmulationMachineModelFunctions.Parse<TModel>(id);
    public EmulationEmulatorDefinition ToDefinition<TEmulator, TMachineModel>(EmulationCoreCatalogEntry<TEmulator, TMachineModel> entry) => entry.ToDefinition();

    /// <inheritdoc/>
    public IReadOnlyList<T> Create<T>(Assembly assembly) where T : class
        => EmulationAdapterFunctions.Create<T>(assembly);

    /// <inheritdoc/>
    public IEnumerable<TFactory> Create<TFactory, TAdapter>(Assembly assembly) where TFactory : class
        => EmulationAdapterFunctions.Create<TFactory, TAdapter>(assembly);
}
