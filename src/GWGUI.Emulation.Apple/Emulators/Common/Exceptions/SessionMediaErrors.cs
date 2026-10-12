namespace GWGUI.Emulation.Apple.Emulators.Common.Exceptions;


internal static class SessionMediaErrors
{
    internal static string PlaylistEntryMissing => ExceptionText.Get("Emulation.Apple.Error.ContentNotFound");
    internal static string PlaylistEmpty => ContentUnsupported();
    internal static string ExplicitSaveRequired => ContentUnsupported();

    private static string ContentUnsupported() => ExceptionText.Get("Emulation.Apple.Error.ContentUnsupported");
}
