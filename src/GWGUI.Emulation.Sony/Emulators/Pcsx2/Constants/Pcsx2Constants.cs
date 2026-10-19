namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Constants;

internal static class Pcsx2Constants
{
    internal const string LibraryFileName = "pcsx2_libretro.dll";
    internal const string OfficialPackageUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/pcsx2_libretro.dll.zip";
    internal static CoreReleaseSettings ReleaseSettings { get; } = new(new Uri(OfficialPackageUrl), LibraryFileName, LibraryFileName, DisplayName);

    internal const string Id = "pcsx2";
    internal const string DisplayName = "PCSX2";
    internal const string LibraryName = "PCSX2";
    internal const string CurrentLibraryName = "LRPS2";
    internal const string DescriptionResourceKey = "Emulation.Emulator.pcsx2.Description";
    internal const string CoreHostCommand = "--sony-pcsx2-core-host";
}
