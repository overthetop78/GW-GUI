using Hardware = GWGUI.Emulation.Commodore.Common.Machines.C64.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceX64Sc.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "C64";
    internal const string Id = "vice_x64sc";
    internal const string DisplayName = "VICE x64sc";
    internal const string LibraryFile = "vice_x64sc_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_x64sc_libretro.dll.zip";
    internal const string ModelOption = "vice_c64_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.C64] = ("C64 PAL", "c64"),
        };
}
