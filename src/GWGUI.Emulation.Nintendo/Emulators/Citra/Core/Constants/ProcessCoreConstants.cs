using GWGUI.Emulation.Nintendo.Emulators.Citra.Constants;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Factories;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Functions;
using GWGUI.Emulation.Nintendo.Emulators.Citra.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = CitraConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-citra-";
    internal const string VideoMapPrefix = "gwgui-nintendo-citra-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

