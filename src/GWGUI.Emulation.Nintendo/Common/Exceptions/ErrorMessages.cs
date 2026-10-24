namespace GWGUI.Emulation.Nintendo.Common.Exceptions;

/// <summary>Messages de validation et d'exécution du module, résolus par ExceptionText.</summary>
internal static class ErrorMessages
{

    internal static string HostConfigurationInvalid() => ExceptionText.Get("Emulation.Error.Core.HostConfigurationInvalid");
    internal static string HostNotInitialized() => ExceptionText.Get("Emulation.Error.Core.HostNotInitialized");
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
    internal static string ProcessAlreadyInitialized() => ExceptionText.Get("Emulation.Error.Core.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => ExceptionText.Get("Emulation.Error.Core.HostExecutableNotFound");
    internal static string ProcessStartFailed() => ExceptionText.Get("Emulation.Error.Core.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => ExceptionText.Get("Emulation.Error.Core.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => ExceptionText.Get("Emulation.Error.Core.ProcessUnavailable");
    internal static string ProcessNotInitialized() => ExceptionText.Get("Emulation.Error.Core.ProcessNotInitialized");
    internal static string ProcessTimeout() => ExceptionText.Get("Emulation.Error.Core.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) => ExceptionText.Get("Emulation.Error.Core.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => ExceptionText.Get("Emulation.Error.Core.HostResponseUnavailable");
    internal static string UnknownCoreOption() => ExceptionText.Get("Emulation.Error.Core.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => ExceptionText.Get("Emulation.Error.Core.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => ExceptionText.Get("Emulation.Error.Core.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => ExceptionText.Get("Emulation.Error.Core.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => ExceptionText.Get("Emulation.Error.Core.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => ExceptionText.Get("Emulation.Error.Core.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => ExceptionText.Get("Emulation.Error.Core.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => ExceptionText.Get("Emulation.Error.Core.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => ExceptionText.Get("Emulation.Error.Core.UnknownHostCommand", command);

}


