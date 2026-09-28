using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Contracts;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Factories;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Functions;
using GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Services;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Constants;

internal static class ProcessCoreConstants
{
    internal const string CoreHost = BeetlePcfxConstants.CoreHostCommand;
    internal const string PipePrefix = "gwgui-nec-beetle-pcfx-";
    internal const string VideoMapPrefix = "gwgui-nec-beetle-pcfx-video-";
    internal const int PipeBufferSize = 8 * 1024 * 1024;
}

