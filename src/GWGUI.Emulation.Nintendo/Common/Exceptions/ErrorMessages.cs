namespace GWGUI.Emulation.Nintendo.Common.Exceptions;

/// <summary>Messages de validation et d'exécution du module, résolus par ExceptionText.</summary>
internal static class ErrorMessages
{

    internal static string CoreNotInitialized() => ExceptionText.Get("Emulation.Error.Core.CoreNotInitialized");
    internal static string MediaNotFound() => ExceptionText.Get("Emulation.Error.Core.MediaNotFound");
    internal static string StartWithoutMediaUnsupported() => ExceptionText.Get("Emulation.Error.Core.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => ExceptionText.Get("Emulation.Error.Core.ContentRefused");
    internal static string PlaylistLimitExceeded() => ExceptionText.Get("Emulation.Error.Core.PlaylistLimitExceeded");
    internal static string StateSaveFailed() => ExceptionText.Get("Emulation.Error.Core.StateSaveFailed");
    internal static string StateEmpty() => ExceptionText.Get("Emulation.Error.Core.StateEmpty");
    internal static string StateRestoreFailed() => ExceptionText.Get("Emulation.Error.Core.StateRestoreFailed");
    internal static string CoreNotLoaded() => ExceptionText.Get("Emulation.Error.Core.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => ExceptionText.Get("Emulation.Error.Core.CorePathNotAbsolute");
    internal static string CoreNotFound() => ExceptionText.Get("Emulation.Error.Core.CoreNotFound");
    internal static string ArchiveMissingLibrary() => ExceptionText.Get("Emulation.Error.Core.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => ExceptionText.Get("Emulation.Error.Core.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => ExceptionText.Get("Emulation.Error.Core.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => ExceptionText.Get("Emulation.Error.Core.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => ExceptionText.Get("Emulation.Error.Core.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => ExceptionText.Get("Emulation.Error.Core.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => ExceptionText.Get("Emulation.Error.Core.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => ExceptionText.Get("Emulation.Error.Core.MediaSlotCreationFailed");
    internal static string MediaRefused() => ExceptionText.Get("Emulation.Error.Core.MediaRefused");
    internal static string MediaSelectionFailed() => ExceptionText.Get("Emulation.Error.Core.MediaSelectionFailed");
    internal static string MediaInsertFailed() => ExceptionText.Get("Emulation.Error.Core.MediaInsertFailed");
    internal static string DiskControlUnavailable() => ExceptionText.Get("Emulation.Error.Core.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => ExceptionText.Get("Emulation.Error.Core.DiskControlIncomplete");
    internal static string UnknownCoreOption() => ExceptionText.Get("Emulation.Error.Core.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => ExceptionText.Get("Emulation.Error.Core.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => ExceptionText.Get("Emulation.Error.Core.InvalidOptionValue", value, key);
    internal static string UnsupportedApiVersion(uint version) => ExceptionText.Get("Emulation.Error.Core.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => ExceptionText.Get("Emulation.Error.Core.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => ExceptionText.Get("Emulation.Error.Core.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => ExceptionText.Get("Emulation.Error.Core.InvalidStateSize", size);

}


