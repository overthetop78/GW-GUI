using Hardware = GWGUI.Emulation.Commodore.Common.Machines.Plus4.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceXPlus4.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "PLUS4";
    internal const string Id = "vice_xplus4";
    internal const string DisplayName = "VICE xplus4";
    internal const string LibraryFile = "vice_xplus4_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_xplus4_libretro.dll.zip";
    internal const string ModelOption = "vice_plus4_model";
    internal const string C16PalModel = "C16 PAL";
    internal const string C16PalCommand = "c16pal";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.C16] = (C16PalModel, C16PalCommand),
            [Hardware.Plus4] = ("PLUS4 PAL", "plus4pal"),
            [Hardware.V364] = ("V364 NTSC", "v364"),
            [Hardware.C232] = ("232 NTSC", "c232"),
        };
}
