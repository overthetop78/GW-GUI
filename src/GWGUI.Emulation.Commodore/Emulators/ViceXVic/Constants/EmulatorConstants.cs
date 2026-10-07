using Hardware = GWGUI.Emulation.Commodore.Common.Machines.Vic20.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceXVic.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "VIC20";
    internal const string Id = "vice_xvic";
    internal const string DisplayName = "VICE xvic";
    internal const string LibraryFile = "vice_xvic_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_xvic_libretro.dll.zip";
    internal const string ModelOption = "vice_vic20_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.Vic20] = ("VIC20 PAL", "vic20pal"),
            [Hardware.Vic21] = ("VIC21", "vic21"),
        };
}
