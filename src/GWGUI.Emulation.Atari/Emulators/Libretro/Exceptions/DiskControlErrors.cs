namespace GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;


internal static class DiskControlErrors
{
    internal static string Unavailable => ContentUnsupported();
    internal static string Incomplete => ContentUnsupported();
    internal static string EjectFailed => ContentUnsupported();
    internal static string SelectFailed => ContentUnsupported();
    internal static string InsertFailed => ContentUnsupported();
    internal static string CreateSlotFailed => ContentUnsupported();
    internal static string ReplaceFailed => ContentUnsupported();
    internal static string MediaMissing => ExceptionText.Get("Emulation.Atari.Error.ContentNotFound");

    private static string ContentUnsupported() =>
        ExceptionText.Get("Emulation.Atari.Error.ContentUnsupported");
}
