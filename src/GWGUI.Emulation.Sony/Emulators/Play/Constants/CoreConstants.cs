using GWGUI.Emulation.Sony.Emulators.Common.Interop.Contracts;
using GWGUI.Emulation.Sony.Emulators.Play.Dictionaries;

namespace GWGUI.Emulation.Sony.Emulators.Play.Constants;

internal static class CoreConstants
{
    internal const string Id = "play";
    internal const string DisplayName = "Play!";
    internal const string LibraryName = "Play!";
    internal const string LibraryFileName = "play_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/play_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.play.Description";
    internal static CoreDefinition Definition { get; } = new(
        Id, LibraryName, LibraryFileName, FirmwareConstants.ContentExtensions,
        new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionCatalog.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.PlayStation2 }),
        RequiresExternalFirmware: false,
        ControllerTypes: [ControllerType.Joystick, ControllerType.None]);
}
