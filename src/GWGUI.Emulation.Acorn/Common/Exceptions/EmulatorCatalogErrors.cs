namespace GWGUI.Emulation.Acorn.Common.Exceptions;

internal static class EmulatorCatalogErrors
{
    internal static string EmptyInstallationRoot => CoreNotFound();
    internal static string EmptyVersion => CoreNotFound();
    internal static string DuplicateCore => CoreRejected();
    internal static string MissingModel => CoreNotFound();

    private static string CoreNotFound() => ExceptionText.Get("Emulation.Acorn.Error.CoreNotFound");
    private static string CoreRejected() => ExceptionText.Get("Emulation.Acorn.Error.CoreRejected");
}

