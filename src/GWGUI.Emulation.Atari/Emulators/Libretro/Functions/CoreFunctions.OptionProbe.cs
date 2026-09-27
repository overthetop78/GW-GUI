
namespace GWGUI.Emulation.Atari.Emulators.Libretro.Functions;

public static class CoreOptionProbe
{
    public static string DescribeFailure(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.GetType().Name;
    }

    public static IReadOnlyList<CoreOption> Inspect(string corePath, Emulator category)
    {
        var absoluteCore = Path.GetFullPath(corePath);
        ExternalCoreProbe.Inspect(absoluteCore, category);
        var session = Path.Combine(Path.GetTempPath(), CoreOptionProbeValues.GWGUIAtariOptionProbe, Guid.NewGuid().ToString(CoreOptionProbeValues.N));
        Directory.CreateDirectory(session);
        try
        {
            using var library = new ExternalCoreLibrary(absoluteCore);
            var exports = CoreFunctions.ResolveExports(library);
            using var callbacks = new ExternalHostCallbacks(category,
                Path.Combine(session, CoreDirectoryConstants.SystemDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.ContentDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.SavesDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.AssetsDirectoryName),
                new Dictionary<string, string>());
            CoreFunctions.InstallCallbacks(exports, callbacks);
            exports.Initialize();
            try
            {
                return callbacks.Options.ToArray();
            }
            finally
            {
                exports.Deinitialize();
            }
        }
        finally
        {
            Directory.Delete(session, recursive: true);
        }
    }
}
