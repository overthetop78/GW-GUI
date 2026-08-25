namespace GWGUI.Emulation.Atari.Constants;

internal static class AtariCoreReleaseErrors
{
    internal const string MissingPublishedDate =
        "The official Atari core source did not provide a build date.";
    internal const string MissingExpectedLibraryFormat =
        "The official Atari core archive does not contain the expected library '{0}'.";
    internal const string InvalidExportDirectory =
        "The downloaded Atari core has an invalid PE export directory.";
    internal const string InstalledLibraryLockedFormat =
        "The installed Atari core library '{0}' is locked and could not be replaced.";
}
