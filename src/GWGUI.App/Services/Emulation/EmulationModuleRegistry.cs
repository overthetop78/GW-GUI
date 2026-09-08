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
    private static readonly Lazy<IReadOnlyList<IEmulationModule>> LoadedModules = new(() => Discover(
        Path.Combine(AppContext.BaseDirectory, "Modules"), StoragePaths.DataDirectory, WriteDiagnostic));
    internal static IReadOnlyList<IEmulationModule> Modules => LoadedModules.Value;

    internal static IEmulationModuleLocalization? FindLocalization(string? moduleId) =>
        string.IsNullOrWhiteSpace(moduleId) ? null : Modules.FirstOrDefault(module =>
            string.Equals(module.Id, moduleId, StringComparison.OrdinalIgnoreCase))
            as IEmulationModuleLocalization;

    internal static IReadOnlyList<IEmulationModule> Discover(string directory, string dataDirectory,
        Action<string, Exception?> diagnostic)
    {
        var modules = new Dictionary<string, IEmulationModule>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!Directory.Exists(directory)) return [];
            foreach (var path in Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly))
                diagnostic($"Ignoring loose module DLL '{path}': a module folder with module.json is required.", null);
            foreach (var path in Directory.EnumerateDirectories(directory)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
                LoadModule(path, dataDirectory, modules, diagnostic);
        }
        catch (Exception error)
        {
            diagnostic($"Discovering emulation modules in '{directory}'", error);
        }
        return modules.Values.OrderBy(module => module.Id, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void LoadModule(string directory, string dataDirectory,
        IDictionary<string, IEmulationModule> modules, Action<string, Exception?> diagnostic)
    {
        var context = $"Loading emulation module manifest '{Path.Combine(directory, EmulationHostApi.ManifestFileName)}'";
        try
        {
            var manifest = EmulationModuleManifestReader.Read(directory);
            var path = Path.Combine(Path.GetFullPath(directory), manifest.EntryAssembly);
            context = $"Loading emulation module '{manifest.Id}' version {manifest.ModuleVersion} from '{path}'";
            if (modules.ContainsKey(manifest.Id))
                throw new InvalidDataException($"An emulation module with id '{manifest.Id}' is already loaded.");
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(path));
            var factories = FactoryTypes(assembly, diagnostic).ToArray();
            if (factories.Length != 1)
                throw new InvalidDataException($"Module '{manifest.Id}' must expose exactly one public factory; found {factories.Length}.");
            var factory = (IEmulationModuleFactory)Activator.CreateInstance(factories[0])!;
            if (!string.Equals(factory.Id, manifest.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Manifest id '{manifest.Id}' does not match factory id '{factory.Id}'.");
            var root = Path.Combine(dataDirectory, "Emulation", "Machines", factory.Id);
            var module = factory.Create(new EmulationModuleContext(dataDirectory, root, HttpClient));
            if (module is null || !string.Equals(module.Id, manifest.Id, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Manifest id '{manifest.Id}' does not match the created module id '{module?.Id}'.");
            modules.Add(manifest.Id, module);
            diagnostic($"Loaded emulation module '{module.Id}' version {manifest.ModuleVersion} from '{path}' " +
                $"(host API {EmulationHostApi.CurrentVersion}).", null);
        }
        catch (Exception error)
        {
            diagnostic(context, error);
        }
    }

    private static IEnumerable<Type> FactoryTypes(Assembly assembly, Action<string, Exception?> diagnostic)
    {
        try
        {
            return assembly.GetTypes().Where(IsFactory).ToArray();
        }
        catch (ReflectionTypeLoadException error)
        {
            foreach (var loaderError in error.LoaderExceptions.OfType<Exception>())
                diagnostic($"Inspecting emulation module assembly '{assembly.Location}'", loaderError);
            return error.Types.OfType<Type>().Where(IsFactory).ToArray();
        }
    }

    private static bool IsFactory(Type type) =>
        type is { IsClass: true, IsAbstract: false, IsPublic: true, ContainsGenericParameters: false }
        && typeof(IEmulationModuleFactory).IsAssignableFrom(type)
        && type.GetConstructor(Type.EmptyTypes) is not null;

    private static void WriteDiagnostic(string context, Exception? error)
    {
        if (error is null) ErrorLog.WriteInformation(context, "Emulation module discovery");
        else ErrorLog.Write(error, context);
    }
}
