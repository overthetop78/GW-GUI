using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.VeMUlator.Constants;

internal static class CoreConstants
{
    internal const string Id = "vemulator";
    internal const string DisplayName = "VeMUlator";
    internal const string LibraryName = "VeMUlator";
    internal const string LibraryFileName = "vemulator_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/vemulator_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.vemulator.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".vms", ".bin", ".dci"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.DreamcastVmu }));
}
