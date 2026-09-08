using GWGUI.App.Services.Storage;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Net.Http;
using GWGUI.Emulation;
using GWGUI.App.Services.Logging;

namespace GWGUI.App.Services.Emulation;

internal static class EmulationModuleRegistry
{
    private static readonly HttpClient HttpClient = new();
    internal static IReadOnlyList<IEmulationModule> Modules { get; } = Discover();

    internal static IEmulationModuleLocalization? FindLocalization(string? moduleId) =>
        string.IsNullOrWhiteSpace(moduleId) ? null : Modules.FirstOrDefault(module =>
            string.Equals(module.Id, moduleId, StringComparison.OrdinalIgnoreCase))
            as IEmulationModuleLocalization;

    private static IReadOnlyList<IEmulationModule> Discover()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "Modules");
        var modules = new Dictionary<string, IEmulationModule>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!Directory.Exists(directory)) return [];
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                LoadAssembly(path, modules);
        }
        catch (Exception error)
        {
            ErrorLog.Write(error, $"Discovering emulation modules in '{directory}'");
        }
        return modules.Values.OrderBy(module => module.Id, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void LoadAssembly(string path, IDictionary<string, IEmulationModule> modules)
    {
        try
        {
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path));
            foreach (var type in FactoryTypes(assembly))
            {
                try
                {
                    var factory = (IEmulationModuleFactory)Activator.CreateInstance(type)!;
                    ValidateId(factory.Id);
                    if (modules.ContainsKey(factory.Id))
                        throw new InvalidDataException($"An emulation module with id '{factory.Id}' is already loaded.");
                    var root = Path.Combine(StoragePaths.EmulationDirectory, "Machines", factory.Id);
                    var module = factory.Create(new EmulationModuleContext(
                        StoragePaths.DataDirectory, root, HttpClient));
                    if (!string.Equals(module.Id, factory.Id, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException(
                            $"Emulation module factory id '{factory.Id}' does not match module id '{module.Id}'.");
                    modules.Add(module.Id, module);
                }
                catch (Exception error)
                {
                    ErrorLog.Write(error, $"Loading emulation module factory '{type.FullName}' from '{path}'");
                }
            }
        }
        catch (Exception error)
        {
            ErrorLog.Write(error, $"Loading emulation module assembly '{path}'");
        }
    }

    private static IEnumerable<Type> FactoryTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes().Where(IsFactory).ToArray();
        }
        catch (ReflectionTypeLoadException error)
        {
            foreach (var loaderError in error.LoaderExceptions.OfType<Exception>())
                ErrorLog.Write(loaderError, $"Inspecting emulation module assembly '{assembly.Location}'");
            return error.Types.OfType<Type>().Where(IsFactory).ToArray();
        }
    }

    private static bool IsFactory(Type type) =>
        type is { IsClass: true, IsAbstract: false, IsPublic: true }
        && typeof(IEmulationModuleFactory).IsAssignableFrom(type)
        && type.GetConstructor(Type.EmptyTypes) is not null;

    private static void ValidateId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || id.Contains(Path.DirectorySeparatorChar) || id.Contains(Path.AltDirectorySeparatorChar))
            throw new InvalidDataException($"Invalid emulation module id '{id}'.");
    }
}
