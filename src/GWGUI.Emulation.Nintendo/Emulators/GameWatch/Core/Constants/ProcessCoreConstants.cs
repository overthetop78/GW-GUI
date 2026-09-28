using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Constants;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Factories;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Functions;
using GWGUI.Emulation.Nintendo.Emulators.GameWatch.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.GameWatch.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = GameWatchConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-gw-";
    internal const string VideoMapPrefix = "gwgui-nintendo-gw-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}



