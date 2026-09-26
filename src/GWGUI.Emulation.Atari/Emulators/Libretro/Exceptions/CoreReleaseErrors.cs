namespace GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;

internal static class CoreReleaseErrors
{
    internal static string MissingPublishedDate => CoreRejected();
    internal static string MissingExpectedLibraryFormat => CoreRejected();
    internal static string InvalidExportDirectory => CoreRejected();
    internal static string InstalledLibraryLockedFormat => CoreRejected();

    private static string CoreRejected() => ExceptionText.Get("Emulation.Atari.Error.CoreRejected");
}
