namespace GWGUI.Emulation.Acorn.Common.Exceptions;

internal static class CoreReleaseErrors
{
    internal static string MissingPublishedDate => CoreRejected();
    internal static string MissingExpectedLibraryFormat => CoreRejected();
    internal static string InvalidExportDirectory => CoreRejected();
    internal static string InstalledLibraryLockedFormat => CoreRejected();

    private static string CoreRejected() => ExceptionText.Get("Emulation.Acorn.Error.CoreRejected");
}

