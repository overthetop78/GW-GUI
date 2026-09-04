using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace GWGUI.Launcher;

internal static class Program
{
    private static readonly string LibraryDirectory = Path.Combine(AppContext.BaseDirectory, "lib");

    [STAThread]
    private static int Main(string[] args)
    {
        AssemblyLoadContext.Default.Resolving += ResolveAssembly;
        AddLibraryDirectoriesToPath();

        var application = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(LibraryDirectory, "gwgui.app.dll"));
        var entryPoint = application.EntryPoint ?? throw new InvalidOperationException("gwgui.app.dll has no entry point.");
        var parameters = entryPoint.GetParameters().Length == 0 ? null : new object?[] { args };
        var result = entryPoint.Invoke(null, parameters);
        return result is int exitCode ? exitCode : 0;
    }

    private static Assembly? ResolveAssembly(AssemblyLoadContext context, AssemblyName name)
    {
        if (name.Name is null) return null;
        if (name.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(name.CultureName))
        {
            var resource = Path.Combine(AppContext.BaseDirectory, "Languages", $"{name.CultureName}.dll");
            if (File.Exists(resource)) return context.LoadFromAssemblyPath(resource);
        }
        if (!Directory.Exists(LibraryDirectory)) return null;
        var path = Directory.EnumerateFiles(LibraryDirectory, $"{name.Name}.dll", SearchOption.AllDirectories).FirstOrDefault();
        return path is null ? null : context.LoadFromAssemblyPath(path);
    }

    private static void AddLibraryDirectoriesToPath()
    {
        if (!Directory.Exists(LibraryDirectory)) return;
        var directories = Directory.EnumerateDirectories(LibraryDirectory, "*", SearchOption.AllDirectories)
            .Prepend(LibraryDirectory);
        var currentPath = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator, directories.Append(currentPath)));
    }
}
