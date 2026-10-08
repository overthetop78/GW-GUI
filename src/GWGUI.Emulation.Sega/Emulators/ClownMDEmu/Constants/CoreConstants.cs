using GWGUI.Emulation.Sega.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Sega.Emulators.ClownMDEmu.Constants;

internal static class CoreConstants
{
    internal const string Id = "clownmdemu";
    internal const string DisplayName = "ClownMDEmu";
    internal const string LibraryName = "ClownMDEmu";
    internal const string LibraryFileName = "clownmdemu_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/clownmdemu_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.clownmdemu.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".bin", ".md", ".gen", ".cue", ".iso", ".chd"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.MegaDrive, ModelConstants.MegaCd }));
}
