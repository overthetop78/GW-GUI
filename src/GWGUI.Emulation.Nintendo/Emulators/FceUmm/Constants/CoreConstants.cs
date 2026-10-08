using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.FceUmm.Constants;

internal static class CoreConstants
{
    internal const string Id = "fceumm";
    internal const string DisplayName = "FCEUmm";
    internal const string LibraryName = "FCEUmm";
    internal const string LibraryFileName = "fceumm_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/fceumm_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.fceumm.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".fds", ".nes", ".unf", ".unif"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.Nes, ModelConstants.FamicomDisk }));
}
