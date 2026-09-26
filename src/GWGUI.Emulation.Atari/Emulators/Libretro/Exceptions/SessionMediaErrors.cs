namespace GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;


internal static class SessionMediaErrors
{
    internal static string PlaylistEntryMissing => ExceptionText.Get("Emulation.Atari.Error.ContentNotFound");
    internal static string PlaylistEmpty => ContentUnsupported();
    internal static string ExplicitSaveRequired => ContentUnsupported();

    private static string ContentUnsupported() =>
        ExceptionText.Get("Emulation.Atari.Error.ContentUnsupported");
}
