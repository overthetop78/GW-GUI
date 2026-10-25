using System.Reflection;

namespace GWGUI.Emulation.Functions;

internal static class EmulationAdapterFunctions
{
    public static IReadOnlyList<T> Create<T>(Assembly assembly) where T : class
    {
        ArgumentNullException.ThrowIfNull(assembly);
        return CreateInstances<T>(assembly.GetTypes().Where(type => !type.IsAbstract && !type.ContainsGenericParameters && typeof(T).IsAssignableFrom(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal)).ToArray();
    }

    internal static IEnumerable<TFactory> Create<TFactory, TAdapter>(Assembly assembly) where TFactory : class =>
        CreateInstances<TFactory>(assembly.GetTypes().Where(type => !type.IsAbstract
            && typeof(TFactory).IsAssignableFrom(type) && typeof(TAdapter).IsAssignableFrom(type)));

    private static IEnumerable<T> CreateInstances<T>(IEnumerable<Type> types) where T : class =>
        types.Select(type => (T)Activator.CreateInstance(type, nonPublic: true)!);
}
