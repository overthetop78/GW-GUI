namespace GWGUI.Emulation.Apple.Common.Dictionaries;

internal static class EmulatorCatalog
{
    private static readonly Type[] AdapterTypes = EmulationAdapterFunctions.Create<IEmulatorAdapter>(typeof(EmulatorCatalog).Assembly).Select(adapter => adapter.GetType()).ToArray();
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => AdapterTypes.Select(type => (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)!).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();
}
