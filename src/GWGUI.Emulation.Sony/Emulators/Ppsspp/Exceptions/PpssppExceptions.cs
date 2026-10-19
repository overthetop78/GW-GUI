using GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;

internal static class PpssppExceptions
{
    internal static string FullContentPathsRequired() => CoreExceptions.ForEmulator(null, "Emulation.Error.Ppsspp.FullContentPathsRequired");
    internal static string SystemFilesMissing() => CoreExceptions.ForEmulator(null, "Emulation.Error.Ppsspp.SystemFilesMissing");
}
