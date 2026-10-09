namespace GWGUI.Emulation.Acorn.Emulators.Common.Exceptions;


internal static class SessionMediaErrors
{
    internal static string PlaylistEntryMissing => ExceptionText.Get("Emulation.Acorn.Error.ContentNotFound");
    internal static string PlaylistEmpty => ContentUnsupported();
    internal static string ExplicitSaveRequired => ContentUnsupported();

    private static string ContentUnsupported() =>
        ExceptionText.Get("Emulation.Acorn.Error.ContentUnsupported");
}

