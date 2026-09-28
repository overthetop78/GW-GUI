using System.Globalization;
using GWGUI.Emulation.Sega.Modules;

namespace GWGUI.Emulation.Sega.Emulators.Flycast.Exceptions;

internal static class FlycastExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SegaEmulationModule).Assembly, "GWGUI.Emulation.Sega.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Flycast.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Flycast.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Flycast.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Flycast.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Flycast.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Flycast.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Flycast.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Flycast.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Flycast.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Flycast.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Flycast.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Flycast.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Flycast.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Flycast.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Flycast.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Flycast.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Flycast.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Flycast.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Flycast.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Flycast.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Flycast.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Flycast.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Flycast.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Flycast.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Flycast.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Flycast.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Flycast.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Flycast.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Flycast.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Flycast.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Flycast.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Flycast.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Flycast.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Flycast.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Flycast.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Flycast.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Flycast.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Flycast.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Flycast.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Flycast.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Flycast.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Flycast.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Flycast.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Flycast.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Flycast.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Flycast.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

