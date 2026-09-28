using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Constants;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Factories;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Functions;
using GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = BeetleVbConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-beetle-vb-";
    internal const string VideoMapPrefix = "gwgui-nintendo-beetle-vb-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

