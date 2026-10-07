using System.Globalization;
using GWGUI.Emulation.Amstrad.Modules;

namespace GWGUI.Emulation.Amstrad.Emulators.Common.Exceptions;

internal static class CommonExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AmstradEmulationModule).Assembly, CommonExceptionsConstants.ResourceManagerBaseName);

    internal static string HostConfigurationInvalid() => Text(CommonExceptionsConstants.HostConfigurationInvalidResourceKey);
    internal static string HostNotInitialized() => Text(CommonExceptionsConstants.HostNotInitializedResourceKey);
    internal static string CoreNotInitialized() => Text(CommonExceptionsConstants.CoreNotInitializedResourceKey);
    internal static string MediaNotFound() => Text(CommonExceptionsConstants.MediaNotFoundResourceKey);
    internal static string FullContentPathsRequired() => Text(CommonExceptionsConstants.FullContentPathsRequiredResourceKey);
    internal static string StartWithoutMediaUnsupported() => Text(CommonExceptionsConstants.StartWithoutMediaUnsupportedResourceKey);
    internal static string ContentRefused() => Text(CommonExceptionsConstants.ContentRefusedResourceKey);
    internal static string PlaylistLimitExceeded() => Text(CommonExceptionsConstants.PlaylistLimitExceededResourceKey);
    internal static string DiskLabelInvalid() => Text(CommonExceptionsConstants.DiskLabelInvalidResourceKey);
    internal static string StateSaveFailed() => Text(CommonExceptionsConstants.StateSaveFailedResourceKey);
    internal static string StateEmpty() => Text(CommonExceptionsConstants.StateEmptyResourceKey);
    internal static string StateRestoreFailed() => Text(CommonExceptionsConstants.StateRestoreFailedResourceKey);
    internal static string CoreNotLoaded() => Text(CommonExceptionsConstants.CoreNotLoadedResourceKey);
    internal static string CorePathNotAbsolute() => Text(CommonExceptionsConstants.CorePathNotAbsoluteResourceKey);
    internal static string CoreNotFound() => Text(CommonExceptionsConstants.CoreNotFoundResourceKey);
    internal static string ArchiveMissingLibrary() => Text(CommonExceptionsConstants.ArchiveMissingLibraryResourceKey);
    internal static string DownloadedCoreNotPe() => Text(CommonExceptionsConstants.DownloadedCoreNotPeResourceKey);
    internal static string DownloadedCoreInvalidPe() => Text(CommonExceptionsConstants.DownloadedCoreInvalidPeResourceKey);
    internal static string DownloadedCoreWrongArchitecture() => Text(CommonExceptionsConstants.DownloadedCoreWrongArchitectureResourceKey);
    internal static string MediaEjectFailed() => Text(CommonExceptionsConstants.MediaEjectFailedResourceKey);
    internal static string RequestedDiskSelectionFailed() => Text(CommonExceptionsConstants.RequestedDiskSelectionFailedResourceKey);
    internal static string RequestedMediaInsertFailed() => Text(CommonExceptionsConstants.RequestedMediaInsertFailedResourceKey);
    internal static string MediaSlotCreationFailed() => Text(CommonExceptionsConstants.MediaSlotCreationFailedResourceKey);
    internal static string MediaRefused() => Text(CommonExceptionsConstants.MediaRefusedResourceKey);
    internal static string MediaSelectionFailed() => Text(CommonExceptionsConstants.MediaSelectionFailedResourceKey);
    internal static string MediaInsertFailed() => Text(CommonExceptionsConstants.MediaInsertFailedResourceKey);
    internal static string DiskControlUnavailable() => Text(CommonExceptionsConstants.DiskControlUnavailableResourceKey);
    internal static string DiskControlIncomplete() => Text(CommonExceptionsConstants.DiskControlIncompleteResourceKey);
    internal static string ProcessAlreadyInitialized() => Text(CommonExceptionsConstants.ProcessAlreadyInitializedResourceKey);
    internal static string HostExecutableNotFound() => Text(CommonExceptionsConstants.HostExecutableNotFoundResourceKey);
    internal static string ProcessStartFailed() => Text(CommonExceptionsConstants.ProcessStartFailedResourceKey);
    internal static string VideoBufferUnavailable() => Text(CommonExceptionsConstants.VideoBufferUnavailableResourceKey);
    internal static string ProcessUnavailable() => Text(CommonExceptionsConstants.ProcessUnavailableResourceKey);
    internal static string ProcessNotInitialized() => Text(CommonExceptionsConstants.ProcessNotInitializedResourceKey);
    internal static string ProcessTimeout() => Text(CommonExceptionsConstants.ProcessTimeoutResourceKey);
    internal static string ProcessCommunicationFailed(string detail) =>
        Text(CommonExceptionsConstants.ProcessCommunicationFailedResourceKey, detail);
    internal static string HostResponseUnavailable() => Text(CommonExceptionsConstants.HostResponseUnavailableResourceKey);
    internal static string UnknownCoreOption() => Text(CommonExceptionsConstants.UnknownCoreOptionResourceKey);
    internal static string UnsupportedPixelFormat(int value) => Text(CommonExceptionsConstants.UnsupportedPixelFormatResourceKey, value);
    internal static string InvalidOptionValue(string value, string key) => Text(CommonExceptionsConstants.InvalidOptionValueResourceKey, value, key);
    internal static string InvalidResponseLength(int length) => Text(CommonExceptionsConstants.InvalidResponseLengthResourceKey, length);
    internal static string UnsupportedApiVersion(uint version) => Text(CommonExceptionsConstants.UnsupportedApiVersionResourceKey, version);
    internal static string LibraryIdentityMismatch(string? name, string expected) =>
        Text(CommonExceptionsConstants.LibraryIdentityMismatchResourceKey, name ?? string.Empty, expected);
    internal static string UnsupportedContentExtension(string extension) => Text(CommonExceptionsConstants.UnsupportedContentExtensionResourceKey, extension);
    internal static string InvalidStateSize(nuint size) => Text(CommonExceptionsConstants.InvalidStateSizeResourceKey, size);
    internal static string UnknownHostCommand(byte command) => Text(CommonExceptionsConstants.UnknownHostCommandResourceKey, command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == BufferConstants.EmptyCollectionCount ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
