using CommonCoreHostFunctions = GWGUI.Emulation.Bandai.Common.Functions.CoreHostFunctions;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Constants;
using GWGUI.Emulation.Bandai.Emulators.Common.Interop.Services;
using System.IO;

namespace GWGUI.Emulation.Bandai.Emulators.Common.Interop.Functions;

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
        var session = Path.Combine(Path.GetTempPath(), CoreOptionProbeValues.GWGUIBandaiOptionProbe, Guid.NewGuid().ToString(CoreOptionProbeValues.N));
        try
        {
            Directory.CreateDirectory(session);
            using var library = CommonCoreHostFunctions.NativeResources.CreateLibrary(absolutePath);
            var exports = CoreFunctions.ResolveExports(library);
            using var callbacks = new ExternalHostCallbacks(emulator,
                Path.Combine(session, CoreDirectoryConstants.SystemDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.ContentDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.SavesDirectoryName),
                Path.Combine(session, CoreDirectoryConstants.AssetsDirectoryName),
                new Dictionary<string, string>());
            // The supported cores publish their options without starting a native machine.
            CoreFunctions.InstallCallbacks(exports, callbacks);
            return callbacks.Options.ToArray();
        }
        finally
        {
            if (Directory.Exists(session)) Directory.Delete(session, recursive: true);
        }
    }
}
