using GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Contracts;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Factories;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Functions;
using GWGUI.Emulation.Sony.Emulators.Ppsspp.Services;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = PpssppConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-sony-ppsspp-";
    internal const string VideoMapPrefix = "gwgui-sony-ppsspp-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

