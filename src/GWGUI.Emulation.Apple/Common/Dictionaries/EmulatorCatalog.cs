namespace GWGUI.Emulation.Apple.Common.Dictionaries;

internal static class EmulatorCatalog
{
    internal static IReadOnlyList<IEmulatorAdapter> CreateAdapters() => typeof(EmulatorCatalog).Assembly.GetTypes().Where(type => !type.IsAbstract && typeof(IEmulatorAdapter).IsAssignableFrom(type)).Select(type => (IEmulatorAdapter)Activator.CreateInstance(type, nonPublic: true)!).OrderBy(adapter => adapter.EmulatorId, StringComparer.Ordinal).ToArray();
}
