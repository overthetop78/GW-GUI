using GWGUI.Emulation.Apple.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Apple.Emulators.Common.Interop.Services;
using System.IO;

namespace GWGUI.Emulation.Apple.Emulators.Common.Interop.Functions;

public static class CoreOptionProbe
{
    public static string DescribeFailure(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.GetType().Name;
    }

    public static IReadOnlyList<CoreOption> Inspect(string corePath, Emulator emulator)
    {
        var absolutePath = Path.GetFullPath(corePath);
        ExternalCoreProbe.Inspect(absolutePath, emulator);
        var session = Path.Combine(Path.GetTempPath(), CoreOptionProbeValues.GWGUIAppleOptionProbe, Guid.NewGuid().ToString(CoreOptionProbeValues.N));
        try
        {
            Directory.CreateDirectory(session);
            using var library = new ExternalCoreLibrary(absolutePath);
            var exports = CoreFunctions.ResolveExports(library);
            using var callbacks = new ExternalHostCallbacks(emulator,
                Path.Combine(session, CoreDirectoryConstants.SystemDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.ContentDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.SavesDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.AssetsDirectoryName),
                new Dictionary<string, string>());
            // Both cores publish options from retro_set_environment without starting a native machine.
            CoreFunctions.InstallCallbacks(exports, callbacks);
            return callbacks.Options.ToArray();
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, recursive: true);
        }
    }
}
