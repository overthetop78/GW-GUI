namespace GWGUI.Emulation.Sharp.Emulators.Common.Exceptions;

internal static class EmulatorCatalogErrors
{
    internal static string EmptyInstallationRoot => CoreNotFound();
    internal static string EmptyVersion => CoreNotFound();
    internal static string DuplicateCore => CoreRejected();
    internal static string DuplicateModel => CoreRejected();
    internal static string MissingModel => CoreNotFound();

    private static string CoreNotFound() => ExceptionText.Get("Emulation.Sharp.Error.CoreNotFound");
    private static string CoreRejected() => ExceptionText.Get("Emulation.Sharp.Error.CoreRejected");
}
