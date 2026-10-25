using Hardware = GWGUI.Emulation.Commodore.Common.Machines.C64.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceXScpu64.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "SCPU64";
    internal const string Id = "vice_xscpu64";
    internal const string DisplayName = "VICE xscpu64";
    internal const string LibraryFile = "vice_xscpu64_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_xscpu64_libretro.dll.zip";
    internal const string ModelOption = "vice_c64_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.C64SuperCpu] = ("C64 PAL", "c64"),
        };
}
