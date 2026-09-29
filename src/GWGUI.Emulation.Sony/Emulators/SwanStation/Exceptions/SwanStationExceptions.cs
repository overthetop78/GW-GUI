using System.Globalization;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Emulation.Sony.Emulators.SwanStation.Exceptions;

internal static class SwanStationExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SonyEmulationModule).Assembly, "GWGUI.Emulation.Sony.Resources.Error");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.SwanStation.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.SwanStation.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.SwanStation.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.SwanStation.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.SwanStation.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.SwanStation.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.SwanStation.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.SwanStation.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.SwanStation.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.SwanStation.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.SwanStation.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.SwanStation.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.SwanStation.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.SwanStation.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.SwanStation.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.SwanStation.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.SwanStation.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.SwanStation.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.SwanStation.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.SwanStation.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.SwanStation.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.SwanStation.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.SwanStation.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.SwanStation.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.SwanStation.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.SwanStation.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.SwanStation.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.SwanStation.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.SwanStation.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.SwanStation.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.SwanStation.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.SwanStation.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.SwanStation.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.SwanStation.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.SwanStation.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.SwanStation.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.SwanStation.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.SwanStation.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.SwanStation.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.SwanStation.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.SwanStation.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.SwanStation.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.SwanStation.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.SwanStation.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.SwanStation.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.SwanStation.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
