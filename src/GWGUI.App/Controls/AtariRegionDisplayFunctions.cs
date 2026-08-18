using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

internal static class AtariRegionDisplayFunctions
{
    internal static string DisplayName(AtariClassicRegion region) => region switch
    {
        AtariClassicRegion.Pal => AtariVideoAudioSettingsConstants.PalValue,
        AtariClassicRegion.Ntsc => AtariVideoAudioSettingsConstants.NtscValue,
        AtariClassicRegion.RegionFree => LocExtension.Get(AtariHardwareSettingsConstants.RegionFreeResource),
        _ => throw new ArgumentOutOfRangeException(nameof(region), region, null)
    };
}
