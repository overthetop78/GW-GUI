using Hardware = GWGUI.Emulation.Commodore.Common.Machines.CbmII.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceXCbm5x0.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "CBM-II";
    internal const string Id = "vice_xcbm5x0";
    internal const string DisplayName = "VICE xcbm5x0";
    internal const string LibraryFile = "vice_xcbm5x0_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_xcbm5x0_libretro.dll.zip";
    internal const string ModelOption = "vice_cbm5x0_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.CbmII510] = ("510 PAL", "510"),
        };
}
