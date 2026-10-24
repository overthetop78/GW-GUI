namespace GWGUI.Emulation.Commodore.Common.Exceptions;

/// <summary>Messages de validation et d'exécution du module, résolus par ExceptionText.</summary>
internal static class ErrorMessages
{

    internal static string HostConfigurationInvalid() => ExceptionText.Get(CoreErrorResourceKeys.HostConfigurationInvalid);
    internal static string HostNotInitialized() => ExceptionText.Get(CoreErrorResourceKeys.HostNotInitialized);
    internal static string CoreNotInitialized() => ExceptionText.Get(CoreErrorResourceKeys.CoreNotInitialized);
    internal static string MediaNotFound() => ExceptionText.Get(CoreErrorResourceKeys.MediaNotFound);
    internal static string FullContentPathsRequired() => ExceptionText.Get(CoreErrorResourceKeys.FullContentPathsRequired);
    internal static string StartWithoutMediaUnsupported() => ExceptionText.Get(CoreErrorResourceKeys.StartWithoutMediaUnsupported);
    internal static string ContentRefused() => ExceptionText.Get(CoreErrorResourceKeys.ContentRefused);
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
    internal static string ProcessAlreadyInitialized() => ExceptionText.Get(CoreErrorResourceKeys.ProcessAlreadyInitialized);
    internal static string HostExecutableNotFound() => ExceptionText.Get(CoreErrorResourceKeys.HostExecutableNotFound);
    internal static string ProcessStartFailed() => ExceptionText.Get(CoreErrorResourceKeys.ProcessStartFailed);
    internal static string VideoBufferUnavailable() => ExceptionText.Get(CoreErrorResourceKeys.VideoBufferUnavailable);
    internal static string ProcessUnavailable() => ExceptionText.Get(CoreErrorResourceKeys.ProcessUnavailable);
    internal static string ProcessNotInitialized() => ExceptionText.Get(CoreErrorResourceKeys.ProcessNotInitialized);
    internal static string ProcessTimeout() => ExceptionText.Get(CoreErrorResourceKeys.ProcessTimeout);
    internal static string ProcessCommunicationFailed(string detail) => ExceptionText.Get(CoreErrorResourceKeys.ProcessCommunicationFailed, detail);
    internal static string HostResponseUnavailable() => ExceptionText.Get(CoreErrorResourceKeys.HostResponseUnavailable);
    internal static string UnknownCoreOption() => ExceptionText.Get(CoreErrorResourceKeys.UnknownCoreOption);
    internal static string UnsupportedPixelFormat(int value) => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedPixelFormat, value);
    internal static string InvalidOptionValue(string value, string key) => ExceptionText.Get(CoreErrorResourceKeys.InvalidOptionValue, value, key);
    internal static string InvalidResponseLength(int length) => ExceptionText.Get(CoreErrorResourceKeys.InvalidResponseLength, length);
    internal static string UnsupportedHardDiskExtension() => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedHardDiskExtension);
    internal static string UnsupportedApiVersion(uint version) => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedApiVersion, version);
    internal static string LibraryIdentityMismatch(string? name) => ExceptionText.Get(CoreErrorResourceKeys.LibraryIdentityMismatch, name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedContentExtension, extension);
    internal static string InvalidStateSize(nuint size) => ExceptionText.Get(CoreErrorResourceKeys.InvalidStateSize, size);
    internal static string UnsupportedController(string name, int port) => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedController, name, port);
    internal static string UnknownHostCommand(byte command) => ExceptionText.Get(CoreErrorResourceKeys.UnknownHostCommand, command);
}
