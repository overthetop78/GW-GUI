namespace GWGUI.Emulation.Amstrad.Common.Exceptions;

/// <summary>Messages de validation et d'exécution du module, résolus par ExceptionText.</summary>
internal static class ErrorMessages
{

    internal static string CoreNotInitialized() => ExceptionText.Get(CoreErrorResourceKeys.CoreNotInitialized);
    internal static string MediaNotFound() => ExceptionText.Get(CoreErrorResourceKeys.MediaNotFound);
    internal static string StartWithoutMediaUnsupported() => ExceptionText.Get(CoreErrorResourceKeys.StartWithoutMediaUnsupported);
    internal static string ContentRefused() => ExceptionText.Get(CoreErrorResourceKeys.ContentRefused);
    internal static string PlaylistLimitExceeded() => ExceptionText.Get(CoreErrorResourceKeys.PlaylistLimitExceeded);
    internal static string StateSaveFailed() => ExceptionText.Get(CoreErrorResourceKeys.StateSaveFailed);
    internal static string StateEmpty() => ExceptionText.Get(CoreErrorResourceKeys.StateEmpty);
    internal static string StateRestoreFailed() => ExceptionText.Get(CoreErrorResourceKeys.StateRestoreFailed);
    internal static string CoreNotLoaded() => ExceptionText.Get(CoreErrorResourceKeys.CoreNotLoaded);
    internal static string CorePathNotAbsolute() => ExceptionText.Get(CoreErrorResourceKeys.CorePathNotAbsolute);
    internal static string CoreNotFound() => ExceptionText.Get(CoreErrorResourceKeys.CoreNotFound);
    internal static string ArchiveMissingLibrary() => ExceptionText.Get(CoreErrorResourceKeys.ArchiveMissingLibrary);
    internal static string DownloadedCoreNotPe() => ExceptionText.Get(CoreErrorResourceKeys.DownloadedCoreNotPe);
    internal static string DownloadedCoreInvalidPe() => ExceptionText.Get(CoreErrorResourceKeys.DownloadedCoreInvalidPe);
    internal static string DownloadedCoreWrongArchitecture() => ExceptionText.Get(CoreErrorResourceKeys.DownloadedCoreWrongArchitecture);
    internal static string MediaEjectFailed() => ExceptionText.Get(CoreErrorResourceKeys.MediaEjectFailed);
    internal static string RequestedDiskSelectionFailed() => ExceptionText.Get(CoreErrorResourceKeys.RequestedDiskSelectionFailed);
    internal static string RequestedMediaInsertFailed() => ExceptionText.Get(CoreErrorResourceKeys.RequestedMediaInsertFailed);
    internal static string MediaSlotCreationFailed() => ExceptionText.Get(CoreErrorResourceKeys.MediaSlotCreationFailed);
    internal static string MediaRefused() => ExceptionText.Get(CoreErrorResourceKeys.MediaRefused);
    internal static string MediaSelectionFailed() => ExceptionText.Get(CoreErrorResourceKeys.MediaSelectionFailed);
    internal static string MediaInsertFailed() => ExceptionText.Get(CoreErrorResourceKeys.MediaInsertFailed);
    internal static string DiskControlUnavailable() => ExceptionText.Get(CoreErrorResourceKeys.DiskControlUnavailable);
    internal static string DiskControlIncomplete() => ExceptionText.Get(CoreErrorResourceKeys.DiskControlIncomplete);
    internal static string UnknownCoreOption() => ExceptionText.Get(CoreErrorResourceKeys.UnknownCoreOption);
    internal static string UnsupportedPixelFormat(int value) => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedPixelFormat, value);
    internal static string InvalidOptionValue(string value, string key) => ExceptionText.Get(CoreErrorResourceKeys.InvalidOptionValue, value, key);
    internal static string UnsupportedApiVersion(uint version) => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedApiVersion, version);
    internal static string LibraryIdentityMismatch(string? name, string expected) => ExceptionText.Get(CoreErrorResourceKeys.LibraryIdentityMismatch, name ?? string.Empty, expected);
    internal static string UnsupportedContentExtension(string extension) => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedContentExtension, extension);
    internal static string InvalidStateSize(nuint size) => ExceptionText.Get(CoreErrorResourceKeys.InvalidStateSize, size);

}
