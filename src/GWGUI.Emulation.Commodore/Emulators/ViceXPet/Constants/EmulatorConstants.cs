using Hardware = GWGUI.Emulation.Commodore.Common.Machines.Pet.Constants.ModelConstants;

namespace GWGUI.Emulation.Commodore.Emulators.ViceXPet.Constants;

internal static class EmulatorConstants
{
    internal const string ResourceSection = "PET";
    internal const string Id = "vice_xpet";
    internal const string DisplayName = "VICE xpet";
    internal const string LibraryFile = "vice_xpet_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vice_xpet_libretro.dll.zip";
    internal const string ModelOption = "vice_pet_model";
    internal static IReadOnlyDictionary<string, (string Option, string Command)> Models { get; } =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            [Hardware.Pet2001] = ("2001", "2001"),
            [Hardware.Pet3008] = ("3008", "3008"),
            [Hardware.Pet3016] = ("3016", "3016"),
            [Hardware.Pet3032] = ("3032", "3032"),
            [Hardware.Pet3032B] = ("3032B", "3032B"),
            [Hardware.Pet4016] = ("4016", "4016"),
            [Hardware.Pet4032] = ("4032", "4032"),
            [Hardware.Pet4032B] = ("4032B", "4032B"),
            [Hardware.Pet8032] = ("8032", "8032"),
            [Hardware.Pet8096] = ("8096", "8096"),
            [Hardware.Pet8296] = ("8296", "8296"),
            [Hardware.SuperPet] = ("SUPERPET", "SuperPET"),
        };
}
