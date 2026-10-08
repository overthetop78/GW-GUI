using GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Constants;

internal static class CoreConstants
{
    internal const string Id = "beetle_vb";
    internal const string DisplayName = "Beetle VB";
    internal const string LibraryName = "Beetle VB";
    internal const string LibraryFileName = "mednafen_vb_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_vb_libretro.dll.zip";
    internal const string DescriptionResourceKey = "Emulation.Emulator.beetle_vb.Description";
    internal static IReadOnlyList<string> Extensions { get; } = [".vb", ".vboy", ".bin"];
    internal static CoreDefinition Definition { get; } = new(Id, LibraryName, LibraryFileName,
        Extensions, new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName),
        OptionConstants.All, FirmwareConstants.All,
        new(Id, DisplayName, DescriptionResourceKey,
            new HashSet<string>(StringComparer.Ordinal) { ModelConstants.VirtualBoy }));
}
