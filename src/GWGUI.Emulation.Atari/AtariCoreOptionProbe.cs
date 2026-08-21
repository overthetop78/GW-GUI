using GWGUI.Emulation.Atari.Cores;
using GWGUI.Emulation.Common;

namespace GWGUI.Emulation.Atari;

public static class AtariCoreOptionProbe
{
    public static string DescribeFailure(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.GetType().Name;
    }

    public static IReadOnlyList<AtariCoreOption> Inspect(string corePath, AtariEmulator category)
    {
        var absoluteCore = Path.GetFullPath(corePath);
        AtariExternalCoreProbe.Inspect(absoluteCore, category);
        var session = Path.Combine(Path.GetTempPath(), "GWGUI-Atari-OptionProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(session);
        try
        {
            using var library = new ExternalCoreLibrary(absoluteCore);
            var exports = AtariCoreFunctions.ResolveExports(library);
            using var callbacks = new AtariExternalHostCallbacks(
                Path.Combine(session, AtariConstants.SystemDirectoryName),
                Path.Combine(session, AtariConstants.ContentDirectoryName),
                Path.Combine(session, AtariConstants.SavesDirectoryName),
                Path.Combine(session, AtariConstants.AssetsDirectoryName),
                new Dictionary<string, string>());
            AtariCoreFunctions.InstallCallbacks(exports, callbacks);
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
