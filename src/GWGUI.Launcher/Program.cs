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

        return LauncherPolicy.Run(AppContext.BaseDirectory, args,
            path => AssemblyLoadContext.Default.LoadFromAssemblyPath(path).EntryPoint);
    }

    private static Assembly? ResolveAssembly(AssemblyLoadContext context, AssemblyName name)
    {
        var path = LauncherPolicy.ResolveAssemblyPath(AppContext.BaseDirectory, name, File.Exists, Directory.Exists,
            (directory, pattern) => Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories));
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
