namespace GWGUI.Emulation.Amstrad.Common.Exceptions;

/// <summary>Messages de validation et d'exécution du module, résolus par ExceptionText.</summary>
internal static class ErrorMessages
{

    internal static string CoreNotInitialized() => ExceptionText.Get(CommonExceptionsConstants.CoreNotInitializedResourceKey);
    internal static string MediaNotFound() => ExceptionText.Get(CommonExceptionsConstants.MediaNotFoundResourceKey);
    internal static string StartWithoutMediaUnsupported() => ExceptionText.Get(CommonExceptionsConstants.StartWithoutMediaUnsupportedResourceKey);
    internal static string ContentRefused() => ExceptionText.Get(CommonExceptionsConstants.ContentRefusedResourceKey);
    internal static string PlaylistLimitExceeded() => ExceptionText.Get(CommonExceptionsConstants.PlaylistLimitExceededResourceKey);
    internal static string StateSaveFailed() => ExceptionText.Get(CommonExceptionsConstants.StateSaveFailedResourceKey);
    internal static string StateEmpty() => ExceptionText.Get(CommonExceptionsConstants.StateEmptyResourceKey);
    internal static string StateRestoreFailed() => ExceptionText.Get(CommonExceptionsConstants.StateRestoreFailedResourceKey);
    internal static string CoreNotLoaded() => ExceptionText.Get(CommonExceptionsConstants.CoreNotLoadedResourceKey);
    internal static string CorePathNotAbsolute() => ExceptionText.Get(CommonExceptionsConstants.CorePathNotAbsoluteResourceKey);
    internal static string CoreNotFound() => ExceptionText.Get(CommonExceptionsConstants.CoreNotFoundResourceKey);
    internal static string ArchiveMissingLibrary() => ExceptionText.Get(CommonExceptionsConstants.ArchiveMissingLibraryResourceKey);
    internal static string DownloadedCoreNotPe() => ExceptionText.Get(CommonExceptionsConstants.DownloadedCoreNotPeResourceKey);
    internal static string DownloadedCoreInvalidPe() => ExceptionText.Get(CommonExceptionsConstants.DownloadedCoreInvalidPeResourceKey);
    internal static string DownloadedCoreWrongArchitecture() => ExceptionText.Get(CommonExceptionsConstants.DownloadedCoreWrongArchitectureResourceKey);
    internal static string MediaEjectFailed() => ExceptionText.Get(CommonExceptionsConstants.MediaEjectFailedResourceKey);
    internal static string RequestedDiskSelectionFailed() => ExceptionText.Get(CommonExceptionsConstants.RequestedDiskSelectionFailedResourceKey);
    internal static string RequestedMediaInsertFailed() => ExceptionText.Get(CommonExceptionsConstants.RequestedMediaInsertFailedResourceKey);
    internal static string MediaSlotCreationFailed() => ExceptionText.Get(CommonExceptionsConstants.MediaSlotCreationFailedResourceKey);
    internal static string MediaRefused() => ExceptionText.Get(CommonExceptionsConstants.MediaRefusedResourceKey);
    internal static string MediaSelectionFailed() => ExceptionText.Get(CommonExceptionsConstants.MediaSelectionFailedResourceKey);
    internal static string MediaInsertFailed() => ExceptionText.Get(CommonExceptionsConstants.MediaInsertFailedResourceKey);
    internal static string DiskControlUnavailable() => ExceptionText.Get(CommonExceptionsConstants.DiskControlUnavailableResourceKey);
    internal static string DiskControlIncomplete() => ExceptionText.Get(CommonExceptionsConstants.DiskControlIncompleteResourceKey);
    internal static string UnknownCoreOption() => ExceptionText.Get(CommonExceptionsConstants.UnknownCoreOptionResourceKey);
    internal static string UnsupportedPixelFormat(int value) => ExceptionText.Get(CommonExceptionsConstants.UnsupportedPixelFormatResourceKey, value);
    internal static string InvalidOptionValue(string value, string key) => ExceptionText.Get(CommonExceptionsConstants.InvalidOptionValueResourceKey, value, key);
    internal static string UnsupportedApiVersion(uint version) => ExceptionText.Get(CommonExceptionsConstants.UnsupportedApiVersionResourceKey, version);
    internal static string LibraryIdentityMismatch(string? name, string expected) => ExceptionText.Get(CommonExceptionsConstants.LibraryIdentityMismatchResourceKey, name ?? string.Empty, expected);
    internal static string UnsupportedContentExtension(string extension) => ExceptionText.Get(CommonExceptionsConstants.UnsupportedContentExtensionResourceKey, extension);
    internal static string InvalidStateSize(nuint size) => ExceptionText.Get(CommonExceptionsConstants.InvalidStateSizeResourceKey, size);

}
