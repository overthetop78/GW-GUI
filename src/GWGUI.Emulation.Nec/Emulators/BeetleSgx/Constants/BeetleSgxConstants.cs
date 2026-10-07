using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.BeetleSgx.Constants;

internal static class BeetleSgxConstants
{
    internal const string Id = "beetle_sgx";
    internal const string DisplayName = "Beetle SuperGrafx";
    internal const string LibraryName = "Beetle SuperGrafx";
    internal const string DescriptionResourceKey = "Emulation.Emulator.beetle_sgx.Description";
    internal const string OfficialArchiveUrl =
        "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_supergrafx_libretro.dll.zip";
    internal const string ArchiveLibraryName = "mednafen_supergrafx_libretro.dll";
    internal const string InstalledLibraryName = "beetle_sgx_libretro.dll";
    internal static readonly CoreReleaseSettings ReleaseSettings = new(
        new Uri(OfficialArchiveUrl), ArchiveLibraryName, InstalledLibraryName, DisplayName);
}
