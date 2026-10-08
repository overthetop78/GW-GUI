using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.Nestopia.Constants;

internal static class CoreConstants
{
    internal const string Id = "nestopia";
    internal const string DisplayName = "Nestopia";
    internal const string LibraryName = "Nestopia";
    internal const string LibraryFileName = "nestopia_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/nestopia_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.nestopia.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".nes", ".fds", ".unf", ".unif", ".nsf"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes, ModelConstants.FamicomDisk }));
}
