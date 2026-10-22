namespace GWGUI.Emulation.Snk.Common.Exceptions;

internal static class EmulatorCatalogErrors
{
    internal static string EmptyInstallationRoot => CoreNotFound();
    internal static string EmptyVersion => CoreNotFound();
    internal static string DuplicateCore => CoreRejected();

    private static string CoreNotFound() => ExceptionText.Get("Emulation.Snk.Error.CoreNotFound");
    private static string CoreRejected() => ExceptionText.Get("Emulation.Snk.Error.CoreRejected");
}
