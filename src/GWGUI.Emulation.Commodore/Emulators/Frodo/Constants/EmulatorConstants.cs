namespace GWGUI.Emulation.Commodore.Emulators.Frodo.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "frodo";
    internal const string DisplayName = "Frodo";
    internal const string LibraryFile = "frodo_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/frodo_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.Frodo.Description";
    internal const string BasicField = "configuration.rom.Basic";
    internal const string KernalField = "configuration.rom.Kernal";
    internal const string CharactersField = "configuration.rom.Characters";
    internal const string DriveField = "configuration.rom.Drive1541";
    internal const string BasicResourceKey = "Emulation.Commodore.Rom.Basic";
    internal const string KernalResourceKey = "Emulation.Commodore.Rom.Kernal";
    internal const string CharactersResourceKey = "Emulation.Commodore.Rom.Characters";
    internal const string DriveResourceKey = "Emulation.Commodore.Rom.Drive1541";
    internal static IReadOnlyList<(FirmwareSlot Slot, string FileName)> Roms { get; } =
    [
        (new(BasicField, BasicResourceKey), "Basic ROM"),
        (new(KernalField, KernalResourceKey), "Kernal ROM"),
        (new(CharactersField, CharactersResourceKey), "Char ROM"),
        (new(DriveField, DriveResourceKey), "1541 ROM")
    ];
    internal const string OptionPrefix = "frodo_";
    internal const int MaximumMountedMedia = 1;
    internal const int PrimaryMediaIndex = 0;
    internal const uint NoControllerDevice = 0;
    internal const uint JoypadDevice = 1;
    internal static IReadOnlyList<string> DiskExtensions { get; } = [".d64", ".x64", ".lnx", ".lyx", ".zip"];
    internal static IReadOnlyList<string> CassetteExtensions { get; } = [".t64"];
}
