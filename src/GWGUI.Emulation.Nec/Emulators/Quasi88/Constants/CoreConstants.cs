using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.Quasi88.Constants;

internal static class CoreConstants
{
    internal const string Id = "quasi88";
    internal const string DisplayName = "QUASI88";
    internal const string LibraryName = "QUASI88";
    internal const string DescriptionResourceKey = "Emulation.Emulator.quasi88.Description";
    internal const string LibraryFileName = "quasi88_libretro.dll";
    internal const string OfficialArchiveUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/quasi88_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialArchiveUrl),
        LibraryFileName, LibraryFileName, DisplayName);
}
