using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Constants;

internal static class CoreConstants
{
    internal const string Id = "beetle_pce";
    internal const string DisplayName = "Beetle PCE";
    internal const string LibraryName = "Beetle PCE";
    internal const string DescriptionResourceKey = "Emulation.Emulator.beetle_pce.Description";
    internal const string LibraryFileName = "mednafen_pce_libretro.dll";
    internal const string OfficialArchiveUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_pce_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialArchiveUrl),
        LibraryFileName, LibraryFileName, DisplayName);
}
