using GWGUI.Emulation.Nintendo.Emulators.Mgba.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Mgba.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Mgba.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = MgbaConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-mgba-";
    internal const string VideoMapPrefix = "gwgui-nintendo-mgba-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

