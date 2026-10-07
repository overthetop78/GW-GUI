namespace GWGUI.Emulation.Commodore.Common.Machines.Common.Constants;

internal static class StateStoreConstants
{
    internal static readonly byte[] Magic = "GWCMDR04"u8.ToArray();
    internal const string Tmp = ".tmp";
    internal const int MaximumHeaderLength = 1024 * 1024;
    internal const string AllFilesPattern = "*";
    internal const int CurrentFormatVersion = 4;
    internal const int MinimumHeaderLength = 0;
    internal const int DirectoryHashBufferSize = 64 * 1024;
    internal const char CanonicalDirectorySeparator = '/';
}
