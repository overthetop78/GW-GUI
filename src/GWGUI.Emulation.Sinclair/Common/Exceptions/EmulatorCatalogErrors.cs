namespace GWGUI.Emulation.Sinclair.Common.Exceptions;

internal static class EmulatorCatalogErrors
{
    internal static string EmptyInstallationRoot => CoreNotFound();
    internal static string EmptyVersion => CoreNotFound();

    private static string CoreNotFound() => ExceptionText.Get("Emulation.Sinclair.Error.CoreNotFound");
}
