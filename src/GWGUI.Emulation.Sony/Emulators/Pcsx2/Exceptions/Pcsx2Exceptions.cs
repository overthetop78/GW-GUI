using GWGUI.Emulation.Sony.Common.Exceptions;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Exceptions;

internal static class Pcsx2Exceptions
{
    internal static string FullContentPathsRequired() => CoreExceptions.ForEmulator(null, "Emulation.Error.Pcsx2.FullContentPathsRequired");
}
