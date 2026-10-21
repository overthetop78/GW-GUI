using System.Reflection;

namespace GWGUI.Emulation.Services;

/// <summary>Adaptation interne des algorithmes de découverte existants au contrat public du SDK.</summary>
internal sealed class EmulationAdapterService : IEmulationAdapters
{
    /// <inheritdoc/>
    public IReadOnlyList<T> Create<T>(Assembly assembly) where T : class
        => EmulationAdapterFunctions.Create<T>(assembly);

    /// <inheritdoc/>
    public IEnumerable<TFactory> Create<TFactory, TAdapter>(Assembly assembly) where TFactory : class
        => EmulationAdapterFunctions.Create<TFactory, TAdapter>(assembly);
}
