using GWGUI.Emulation.Sega.Emulators.Flycast.Constants;
using GWGUI.Emulation.Sega.Emulators.Flycast.Contracts;
using GWGUI.Emulation.Sega.Emulators.Flycast.Factories;
using GWGUI.Emulation.Sega.Emulators.Flycast.Functions;
using GWGUI.Emulation.Sega.Emulators.Flycast.Services;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = FlycastConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-sega-flycast-";
    internal const string VideoMapPrefix = "gwgui-sega-flycast-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}
