using Hardware = GWGUI.Emulation.Commodore.Common.Machines.CbmII.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceXCbm2.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "CBM-II";
    internal const string Id = "vice_xcbm2";
    internal const string DisplayName = "VICE xcbm2";
    internal const string LibraryFile = "vice_xcbm2_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_xcbm2_libretro.dll.zip";
    internal const string ModelOption = "vice_cbm2_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.CbmII610] = ("610 PAL", "610"),
            [Hardware.CbmII620] = ("620 PAL", "620"),
            [Hardware.CbmII620Plus] = ("620PLUS PAL", "620+"),
            [Hardware.CbmII710] = ("710 NTSC", "710"),
            [Hardware.CbmII720] = ("720 NTSC", "720"),
            [Hardware.CbmII720Plus] = ("720PLUS NTSC", "720+"),
        };
}
