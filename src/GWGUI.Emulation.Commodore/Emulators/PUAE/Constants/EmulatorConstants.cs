namespace GWGUI.Emulation.Commodore.Emulators.PUAE.Constants;

internal static class EmulatorConstants
{
    internal const string Id = "puae";
    internal const string DisplayName = "PUAE";
    internal const string LibraryFile = "puae_libretro.dll";
    internal const string DownloadUrl = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/puae_libretro.dll.zip";
    internal const string SourceUrl = "https://github.com/libretro/libretro-uae";
    internal const string ValidatedReleaseId = "validated-96ebfcfc";
    internal const string ValidatedReleaseDisplayName = "96ebfcfc · 31/07/2026 · GW GUI";
    internal static readonly DateTimeOffset ReleasePublishedAt = new(2026, 7, 31, 1, 0, 0, TimeSpan.Zero);
}
