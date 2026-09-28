using GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Contracts;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Factories;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Functions;
using GWGUI.Emulation.Sony.Emulators.SwanStation.Services;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = SwanStationConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-sony-swanstation-";
    internal const string VideoMapPrefix = "gwgui-sony-swanstation-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

