using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.Supermodel.Constants;

internal static class CoreConstants
{
    internal const string Id = "supermodel";
    internal const string DisplayName = "Supermodel";
    internal const string LibraryName = "Supermodel";
    internal const string LibraryFileName = "supermodel_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/supermodel_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.supermodel.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".zip"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Model3 }));
}
