using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Constants;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Contracts;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Factories;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Functions;
using GWGUI.Emulation.Nintendo.Emulators.MelonDs.Services;

namespace GWGUI.Emulation.Nintendo.Emulators.MelonDs.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = MelonDsConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nintendo-melonds-";
    internal const string VideoMapPrefix = "gwgui-nintendo-melonds-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

