using Hardware = GWGUI.Emulation.Commodore.Common.Machines.C128.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceX128.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "C128";
    internal const string Id = "vice_x128";
    internal const string DisplayName = "VICE x128";
    internal const string LibraryFile = "vice_x128_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_x128_libretro.dll.zip";
    internal const string ModelOption = "vice_c128_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.C128] = ("C128 PAL", "c128"),
        };
}
