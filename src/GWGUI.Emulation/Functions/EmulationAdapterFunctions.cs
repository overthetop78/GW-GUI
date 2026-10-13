using System.Reflection;

namespace GWGUI.Emulation.Functions;

public static class EmulationAdapterFunctions
{
    public static IReadOnlyList<T> Create<T>(Assembly assembly) where T : class
    {
        ArgumentNullException.ThrowIfNull(assembly);
        return assembly.GetTypes().Where(type => !type.IsAbstract && !type.ContainsGenericParameters && typeof(T).IsAssignableFrom(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type => (T)Activator.CreateInstance(type, nonPublic: true)!).ToArray();
    }
}
