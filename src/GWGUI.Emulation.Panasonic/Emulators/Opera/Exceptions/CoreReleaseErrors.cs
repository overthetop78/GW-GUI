namespace GWGUI.Emulation.Panasonic.Emulators.Opera.Exceptions;

internal static class CoreReleaseErrors
{
    internal const string InvalidArchitecture = "The downloaded core is not Windows x64.";
    internal const string MissingPublishedDate = "The official response does not identify its publication date.";
    internal const string MissingExpectedLibraryFormat = "The downloaded archive does not contain {0}.";
    internal const string InvalidExportDirectory = "The native export directory is invalid.";
    internal const string InstalledLibraryLockedFormat = "The installed library cannot be replaced: {0}.";
}
