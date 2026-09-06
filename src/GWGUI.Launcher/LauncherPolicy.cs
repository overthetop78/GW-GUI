using System.IO;
using System.Reflection;

namespace GWGUI.Launcher;

internal static class LauncherPolicy
{
    internal static int Run(string baseDirectory, string[] args, Func<string, MethodInfo?> loadEntryPoint)
    {
        var entryPoint = loadEntryPoint(Path.Combine(baseDirectory, "lib", "gwgui.app.dll"))
            ?? throw new InvalidOperationException("gwgui.app.dll has no entry point.");
        var parameters = entryPoint.GetParameters().Length == 0 ? null : new object?[] { args };
        return entryPoint.Invoke(null, parameters) is int exitCode ? exitCode : 0;
    }

    internal static string? ResolveAssemblyPath(string baseDirectory, AssemblyName name,
        Func<string, bool> fileExists, Func<string, bool> directoryExists,
        Func<string, string, IEnumerable<string>> enumerateFiles)
    {
        if (name.Name is null) return null;
        if (name.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(name.CultureName))
        {
            var resource = Path.Combine(baseDirectory, "Languages", $"{name.CultureName}.dll");
            if (fileExists(resource)) return resource;
        }
        var library = Path.Combine(baseDirectory, "lib");
        return directoryExists(library) ? enumerateFiles(library, $"{name.Name}.dll").FirstOrDefault() : null;
    }
}
