using GWGUI.Emulation.Sony.Common.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;

internal static class PpssppExceptions
{
    internal static string FullContentPathsRequired() => ErrorMessages.ForEmulator(null, "Emulation.Error.Ppsspp.FullContentPathsRequired");
    internal static string SystemFilesMissing() => ErrorMessages.ForEmulator(null, "Emulation.Error.Ppsspp.SystemFilesMissing");
}
