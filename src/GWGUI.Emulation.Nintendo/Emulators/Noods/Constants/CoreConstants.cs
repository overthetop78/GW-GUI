using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Noods.Constants;

internal static class CoreConstants
{
    internal const string Id = "noods";
    internal const string DisplayName = "NooDS";
    internal const string LibraryName = "NooDS";
    internal const string LibraryFileName = "noods_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/noods_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.noods.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nds"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.NintendoDs }));
}
