using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using System.Globalization;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;

internal static class CoreExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SonyEmulationModule).Assembly, CoreExceptionsResourceConstants.ResourceName);

    internal static string HostConfigurationInvalid() => Text(CoreExceptionsResourceConstants.HostConfigurationInvalid);
    internal static string HostNotInitialized() => Text(CoreExceptionsResourceConstants.HostNotInitialized);
    internal static string CoreNotInitialized() => Text(CoreExceptionsResourceConstants.CoreNotInitialized);
    internal static string MediaNotFound() => Text(CoreExceptionsResourceConstants.MediaNotFound);
    internal static string StartWithoutMediaUnsupported() => Text(CoreExceptionsResourceConstants.StartWithoutMediaUnsupported);
    internal static string ContentRefused() => Text(CoreExceptionsResourceConstants.ContentRefused);
    internal static string DiskLabelInvalid() => Text(CoreExceptionsResourceConstants.DiskLabelInvalid);
    internal static string StateSaveFailed() => Text(CoreExceptionsResourceConstants.StateSaveFailed);
    internal static string StateEmpty() => Text(CoreExceptionsResourceConstants.StateEmpty);
    internal static string StateRestoreFailed() => Text(CoreExceptionsResourceConstants.StateRestoreFailed);
    internal static string CoreNotLoaded() => Text(CoreExceptionsResourceConstants.CoreNotLoaded);
    internal static string CorePathNotAbsolute() => Text(CoreExceptionsResourceConstants.CorePathNotAbsolute);
    internal static string CoreNotFound() => Text(CoreExceptionsResourceConstants.CoreNotFound);
    internal static string ArchiveMissingLibrary() => Text(CoreExceptionsResourceConstants.ArchiveMissingLibrary);
    internal static string DownloadedCoreNotPe() => Text(CoreExceptionsResourceConstants.DownloadedCoreNotPe);
    internal static string DownloadedCoreInvalidPe() => Text(CoreExceptionsResourceConstants.DownloadedCoreInvalidPe);
    internal static string DownloadedCoreWrongArchitecture() => Text(CoreExceptionsResourceConstants.DownloadedCoreWrongArchitecture);
    internal static string MediaEjectFailed() => Text(CoreExceptionsResourceConstants.MediaEjectFailed);
    internal static string RequestedDiskSelectionFailed() => Text(CoreExceptionsResourceConstants.RequestedDiskSelectionFailed);
    internal static string RequestedMediaInsertFailed() => Text(CoreExceptionsResourceConstants.RequestedMediaInsertFailed);
    internal static string MediaSlotCreationFailed() => Text(CoreExceptionsResourceConstants.MediaSlotCreationFailed);
    internal static string MediaRefused() => Text(CoreExceptionsResourceConstants.MediaRefused);
    internal static string MediaSelectionFailed() => Text(CoreExceptionsResourceConstants.MediaSelectionFailed);
    internal static string MediaInsertFailed() => Text(CoreExceptionsResourceConstants.MediaInsertFailed);
    internal static string DiskControlUnavailable() => Text(CoreExceptionsResourceConstants.DiskControlUnavailable);
    internal static string DiskControlIncomplete() => Text(CoreExceptionsResourceConstants.DiskControlIncomplete);
    internal static string ProcessAlreadyInitialized() => Text(CoreExceptionsResourceConstants.ProcessAlreadyInitialized);
    internal static string HostExecutableNotFound() => Text(CoreExceptionsResourceConstants.HostExecutableNotFound);
    internal static string ProcessStartFailed() => Text(CoreExceptionsResourceConstants.ProcessStartFailed);
    internal static string VideoBufferUnavailable() => Text(CoreExceptionsResourceConstants.VideoBufferUnavailable);
    internal static string ProcessUnavailable() => Text(CoreExceptionsResourceConstants.ProcessUnavailable);
    internal static string ProcessNotInitialized() => Text(CoreExceptionsResourceConstants.ProcessNotInitialized);
    internal static string ProcessTimeout() => Text(CoreExceptionsResourceConstants.ProcessTimeout);
    internal static string ProcessCommunicationFailed(string detail) =>
        Text(CoreExceptionsResourceConstants.ProcessCommunicationFailed, detail);
    internal static string HostResponseUnavailable() => Text(CoreExceptionsResourceConstants.HostResponseUnavailable);
    internal static string UnknownCoreOption() => Text(CoreExceptionsResourceConstants.UnknownCoreOption);
    internal static string UnsupportedPixelFormat(int value) => Text(CoreExceptionsResourceConstants.UnsupportedPixelFormat, value);
    internal static string InvalidOptionValue(string value, string key) => Text(CoreExceptionsResourceConstants.InvalidOptionValue, value, key);
    internal static string InvalidResponseLength(int length) => Text(CoreExceptionsResourceConstants.InvalidResponseLength, length);
    internal static string UnsupportedApiVersion(uint version) => Text(CoreExceptionsResourceConstants.UnsupportedApiVersion, version);
    internal static string LibraryIdentityMismatch(string? name) => Text(CoreExceptionsResourceConstants.LibraryIdentityMismatch, name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text(CoreExceptionsResourceConstants.UnsupportedContentExtension, extension);
    internal static string InvalidStateSize(nuint size) => Text(CoreExceptionsResourceConstants.InvalidStateSize, size);
    internal static string UnknownHostCommand(byte command) => Text(CoreExceptionsResourceConstants.UnknownHostCommand, command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
