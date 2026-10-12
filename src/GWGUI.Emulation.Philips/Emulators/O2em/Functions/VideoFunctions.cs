using GWGUI.Emulation.Philips.Emulators.O2em.Constants;
using Video = GWGUI.Emulation.Philips.Emulators.O2em.Constants.VideoConstants;

namespace GWGUI.Emulation.Philips.Emulators.O2em.Functions;

internal static class VideoSettingsFunctions
{
    internal static (int Width, int Height)? GetOutputResolution(MachineConfiguration configuration) =>
        configuration.Options.GetValueOrDefault(OptionConstants.CropOverscan) == OptionConstants.Enabled ? (Video.CroppedWidth, Video.CroppedHeight) : null;
}
