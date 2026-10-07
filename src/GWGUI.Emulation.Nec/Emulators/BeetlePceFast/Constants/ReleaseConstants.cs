using GWGUI.Emulation.Nec.Emulators.Common.Interop.Contracts;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePceFast.Constants;

internal static class ReleaseConstants
{
    internal const string OfficialArchiveUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/mednafen_pce_fast_libretro.dll.zip";
    internal const string ArchiveLibraryName = "mednafen_pce_fast_libretro.dll";
    internal const string InstalledLibraryName = "beetle_pce_fast_libretro.dll";
    internal static CoreReleaseSettings Settings { get; } = new(new Uri(OfficialArchiveUrl),
        ArchiveLibraryName, InstalledLibraryName, BeetlePceFastConstants.DisplayName);
}
