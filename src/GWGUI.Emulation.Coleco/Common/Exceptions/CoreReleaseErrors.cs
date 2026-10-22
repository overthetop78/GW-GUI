namespace GWGUI.Emulation.Coleco.Common.Exceptions;

internal static class CoreReleaseErrors
{
    internal static string InvalidArchitecture => CoreRejected();
    internal static string MissingPublishedDate => CoreRejected();
    internal static string MissingExpectedLibraryFormat => CoreRejected();
    internal static string InstalledLibraryLockedFormat => CoreRejected();

    private static string CoreRejected() => ExceptionText.Get("Emulation.Coleco.Error.CoreRejected");
}
