using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.NekoProjectII.Constants;

internal static class CoreConstants
{
    internal const string Id = "neko_project_ii";
    internal const string DisplayName = "Neko Project II";
    internal const string LibraryName = "Neko Project II";
    internal const string DescriptionResourceKey = "Emulation.Emulator.neko_project_ii.Description";
    internal const string LibraryFileName = "nekop2_libretro.dll";
    internal const string OfficialArchiveUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/nekop2_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialArchiveUrl),
        LibraryFileName, LibraryFileName, DisplayName);
}
