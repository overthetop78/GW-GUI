namespace GWGUI.Emulation.Apple.Common.Dictionaries;

internal static class EmulatorCatalog
{
    private static readonly Type[] AdapterTypes = typeof(EmulatorCatalog).Assembly.GetTypes().Where(type => !type.IsAbstract && typeof(IEmulatorAdapter).IsAssignableFrom(type)).ToArray();
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => AdapterTypes.Select(type => (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)!).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();
}
