using Hardware = GWGUI.Emulation.Commodore.Common.Machines.C64.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceX64Dtv.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "C64DTV";
    internal const string Id = "vice_x64dtv";
    internal const string DisplayName = "VICE x64dtv";
    internal const string LibraryFile = "vice_x64dtv_libretro.dll";
    internal const string? DownloadUrl = null;
    internal const string ModelOption = "vice_c64dtv_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.C64Dtv] = ("DTV3 PAL", "v3pal"),
        };
}
