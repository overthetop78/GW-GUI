using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Thomson.Common.Services;

internal sealed class SharedVideoWriter(string namePrefix)
    : EmulationSharedVideoWriter(namePrefix, CoreHostFunctions.CalculateVideoSlotCapacity,
        CoreHostConstants.VideoMapGenerationSeparator, CoreHostConstants.UniqueNameFormat)
{
}
