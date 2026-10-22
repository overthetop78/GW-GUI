using GWGUI.Emulation.Services;
using GWGUI.Emulation.Atari.Common.Functions;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class SharedVideoWriter(string namePrefix)
    : EmulationSharedVideoWriter(namePrefix, CoreHostFunctions.CalculateVideoSlotCapacity,
        CoreHostConstants.VideoMapGenerationSeparator, CoreHostConstants.UniqueNameFormat)
{
}
