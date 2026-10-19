using GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;
using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Factories;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Functions;
using GWGUI.Emulation.Sony.Emulators.Pcsx2.Services;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = Pcsx2Constants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-sony-pcsx2-";
    internal const string VideoMapPrefix = "gwgui-sony-pcsx2-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
    internal const int GracefulShutdownTimeoutMilliseconds = 5_000;
}

