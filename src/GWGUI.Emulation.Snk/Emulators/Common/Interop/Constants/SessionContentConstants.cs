namespace GWGUI.Emulation.Snk.Emulators.Common.Interop.Constants;

internal static class SessionContentConstants
{
    internal const string AllFilesPattern = "*";
    internal const string NativeLibrary = "kernel32.dll";
    internal const string ExtendedPathPrefix = @"\\?\";
    internal const string UncPathPrefix = @"\\";
    internal const string ExtendedUncPathPrefix = @"\\?\UNC\";
    internal const uint DeleteAccess = 0x00010000;
    internal const uint ShareReadWriteDelete = 7;
    internal const uint OpenExisting = 3;
    internal const uint OpenReparsePoint = 0x00200000;
    internal const int FileDispositionInfoEx = 21;
    internal const uint DeletePosixIgnoreReadOnly = 0x00000013;
}
