using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using GWGUI.Emulation;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.App.Services.Emulation;

internal sealed class EmulationModuleLoadContext : AssemblyLoadContext
{
    private static readonly HashSet<string> SharedAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        typeof(IEmulationModule).Assembly.GetName().Name!,
        typeof(FluxDecoderRegistry).Assembly.GetName().Name!
    };

    private readonly string _entryAssemblyPath;
    private readonly string _moduleDirectory;
    private readonly AssemblyDependencyResolver _resolver;

    internal EmulationModuleLoadContext(string entryAssemblyPath)
        : base($"GWGUI.Emulation.Module:{Path.GetFileNameWithoutExtension(entryAssemblyPath)}", isCollectible: false)
    {
        _entryAssemblyPath = Path.GetFullPath(entryAssemblyPath);
        _moduleDirectory = Path.GetDirectoryName(_entryAssemblyPath)
            ?? throw new ArgumentException("The module entry assembly has no parent directory.", nameof(entryAssemblyPath));
        _resolver = new AssemblyDependencyResolver(_entryAssemblyPath);
    }

    internal Assembly LoadEntryAssembly() => LoadFromAssemblyPath(_entryAssemblyPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is not null && SharedAssemblyNames.Contains(assemblyName.Name))
            return Default.LoadFromAssemblyName(assemblyName);

        var path = _resolver.ResolveAssemblyToPath(assemblyName) ?? DirectDependencyPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(EnsurePackagePath(path));
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? nint.Zero : LoadUnmanagedDllFromPath(EnsurePackagePath(path));
    }

    private string? DirectDependencyPath(AssemblyName assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName.Name)
            || assemblyName.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return null;
        var path = Path.Combine(_moduleDirectory, assemblyName.Name + ".dll");
        return File.Exists(path) ? path : null;
    }

    private string EnsurePackagePath(string path)
    {
        var fullPath = Path.GetFullPath(path);
        var relative = Path.GetRelativePath(_moduleDirectory, fullPath);
        if (Path.IsPathRooted(relative) || relative == ".."
            || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || (File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException($"Module dependency must be a regular file inside '{_moduleDirectory}': '{fullPath}'.");
        return fullPath;
    }
}
