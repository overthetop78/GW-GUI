namespace GWGUI.Emulation.Sharp.Common.Exceptions;

internal static class EmulatorCatalogErrors
{
    internal static string EmptyInstallationRoot => CoreNotFound();
    internal static string EmptyVersion => CoreNotFound();

    private static string CoreNotFound() => ExceptionText.Get("Emulation.Sharp.Error.CoreNotFound");
}
