using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Meteor.Constants;

internal static class CoreConstants
{
    internal const string Id = "meteor";
    internal const string DisplayName = "Meteor";
    internal const string LibraryName = "Meteor GBA";
    internal const string LibraryFileName = "meteor_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/meteor_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.meteor.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".gba"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.GameBoyAdvance }));
}
