using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.Geargrafx.Constants;

internal static class GeargrafxConstants
{
    internal const string Id = "geargrafx";
    internal const string DisplayName = "Geargrafx";
    internal const string LibraryName = "Geargrafx";
    internal const string DescriptionResourceKey = "Emulation.Emulator.geargrafx.Description";
    internal const string OfficialArchiveUrl =
        "https://buildbot.libretro.com/nightly/windows/x86_64/latest/geargrafx_libretro.dll.zip";
    internal const string ArchiveLibraryName = "geargrafx_libretro.dll";
    internal const string InstalledLibraryName = "geargrafx_libretro.dll";
    internal const string MmiExtension = ".mmi";
    internal static readonly CoreReleaseSettings ReleaseSettings = new(
        new Uri(OfficialArchiveUrl), ArchiveLibraryName, InstalledLibraryName, DisplayName);
}
