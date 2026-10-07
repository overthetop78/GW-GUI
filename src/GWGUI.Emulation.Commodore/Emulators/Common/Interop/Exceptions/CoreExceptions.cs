using System.Globalization;
using GWGUI.Emulation.Commodore.Modules;

namespace GWGUI.Emulation.Commodore.Emulators.Common.Interop.Exceptions;

internal static class CoreExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(CommodoreEmulationModule).Assembly, EmulationModuleConstants.LocalizationResourceName);

    internal static string HostConfigurationInvalid() => Text(CoreErrorResourceKeys.HostConfigurationInvalid);
    internal static string HostNotInitialized() => Text(CoreErrorResourceKeys.HostNotInitialized);
    internal static string CoreNotInitialized() => Text(CoreErrorResourceKeys.CoreNotInitialized);
    internal static string KickstartNotFound() => Text(CoreErrorResourceKeys.KickstartNotFound);
    internal static string MediaNotFound() => Text(CoreErrorResourceKeys.MediaNotFound);
    internal static string ExtendedRomNotFound() => Text(CoreErrorResourceKeys.ExtendedRomNotFound);
    internal static string RomKeyNotFound() => Text(CoreErrorResourceKeys.RomKeyNotFound);
    internal static string FullContentPathsRequired() => Text(CoreErrorResourceKeys.FullContentPathsRequired);
    internal static string StartWithoutMediaUnsupported() => Text(CoreErrorResourceKeys.StartWithoutMediaUnsupported);
    internal static string ContentRefused() => Text(CoreErrorResourceKeys.ContentRefused);
    internal static string DiskLabelInvalid() => Text(CoreErrorResourceKeys.DiskLabelInvalid);
    internal static string StateSaveFailed() => Text(CoreErrorResourceKeys.StateSaveFailed);
    internal static string StateEmpty() => Text(CoreErrorResourceKeys.StateEmpty);
    internal static string StateRestoreFailed() => Text(CoreErrorResourceKeys.StateRestoreFailed);
    internal static string CoreNotLoaded() => Text(CoreErrorResourceKeys.CoreNotLoaded);
    internal static string CorePathNotAbsolute() => Text(CoreErrorResourceKeys.CorePathNotAbsolute);
    internal static string CoreNotFound() => Text(CoreErrorResourceKeys.CoreNotFound);
    internal static string ArchiveMissingLibrary() => Text(CoreErrorResourceKeys.ArchiveMissingLibrary);
    internal static string DownloadedCoreNotPe() => Text(CoreErrorResourceKeys.DownloadedCoreNotPe);
    internal static string DownloadedCoreInvalidPe() => Text(CoreErrorResourceKeys.DownloadedCoreInvalidPe);
    internal static string DownloadedCoreWrongArchitecture() => Text(CoreErrorResourceKeys.DownloadedCoreWrongArchitecture);
    internal static string MediaEjectFailed() => Text(CoreErrorResourceKeys.MediaEjectFailed);
    internal static string RequestedDiskSelectionFailed() => Text(CoreErrorResourceKeys.RequestedDiskSelectionFailed);
    internal static string RequestedMediaInsertFailed() => Text(CoreErrorResourceKeys.RequestedMediaInsertFailed);
    internal static string MediaSlotCreationFailed() => Text(CoreErrorResourceKeys.MediaSlotCreationFailed);
    internal static string MediaRefused() => Text(CoreErrorResourceKeys.MediaRefused);
    internal static string MediaSelectionFailed() => Text(CoreErrorResourceKeys.MediaSelectionFailed);
    internal static string MediaInsertFailed() => Text(CoreErrorResourceKeys.MediaInsertFailed);
    internal static string DiskControlUnavailable() => Text(CoreErrorResourceKeys.DiskControlUnavailable);
    internal static string DiskControlIncomplete() => Text(CoreErrorResourceKeys.DiskControlIncomplete);
    internal static string ProcessAlreadyInitialized() => Text(CoreErrorResourceKeys.ProcessAlreadyInitialized);
    internal static string HostExecutableNotFound() => Text(CoreErrorResourceKeys.HostExecutableNotFound);
    internal static string ProcessStartFailed() => Text(CoreErrorResourceKeys.ProcessStartFailed);
    internal static string VideoBufferUnavailable() => Text(CoreErrorResourceKeys.VideoBufferUnavailable);
    internal static string ProcessUnavailable() => Text(CoreErrorResourceKeys.ProcessUnavailable);
    internal static string ProcessNotInitialized() => Text(CoreErrorResourceKeys.ProcessNotInitialized);
    internal static string ProcessTimeout() => Text(CoreErrorResourceKeys.ProcessTimeout);
    internal static string ProcessCommunicationFailed(string detail) =>
        Text(CoreErrorResourceKeys.ProcessCommunicationFailed, detail);
    internal static string HostResponseUnavailable() => Text(CoreErrorResourceKeys.HostResponseUnavailable);
    internal static string UnknownCoreOption() => Text(CoreErrorResourceKeys.UnknownCoreOption);
    internal static string UnsupportedPixelFormat(int value) => Text(CoreErrorResourceKeys.UnsupportedPixelFormat, value);
    internal static string InvalidOptionValue(string value, string key) => Text(CoreErrorResourceKeys.InvalidOptionValue, value, key);
    internal static string InvalidResponseLength(int length) => Text(CoreErrorResourceKeys.InvalidResponseLength, length);
    internal static string UnsupportedHardDiskExtension() => Text(CoreErrorResourceKeys.UnsupportedHardDiskExtension);
    internal static string UnsupportedApiVersion(uint version) => Text(CoreErrorResourceKeys.UnsupportedApiVersion, version);
    internal static string LibraryIdentityMismatch(string? name) => Text(CoreErrorResourceKeys.LibraryIdentityMismatch, name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text(CoreErrorResourceKeys.UnsupportedContentExtension, extension);
    internal static string InvalidStateSize(nuint size) => Text(CoreErrorResourceKeys.InvalidStateSize, size);
    internal static string UnsupportedController(string name, int port) => Text(CoreErrorResourceKeys.UnsupportedController, name, port);
    internal static string UnknownHostCommand(byte command) => Text(CoreErrorResourceKeys.UnknownHostCommand, command);
    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == BufferConstants.EmptyCollectionCount
            ? value
            : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
