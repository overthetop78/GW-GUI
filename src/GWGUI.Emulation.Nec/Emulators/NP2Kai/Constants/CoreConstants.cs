using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.NP2Kai.Constants;

internal static class CoreConstants
{
    internal const string Id = "np2kai";
    internal const string DisplayName = "NP2Kai";
    internal const string LibraryName = "Neko Project II kai";
    internal const string DescriptionResourceKey = "Emulation.Emulator.np2kai.Description";
    internal const string LibraryFileName = "np2kai_libretro.dll";
    internal const string OfficialArchiveUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/np2kai_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialArchiveUrl),
        LibraryFileName, LibraryFileName, DisplayName);
}
