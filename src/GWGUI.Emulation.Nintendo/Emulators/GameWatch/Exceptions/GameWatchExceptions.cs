using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.GameWatch.Exceptions;

internal static class GameWatchExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.GameWatch.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.GameWatch.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.GameWatch.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.GameWatch.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.GameWatch.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.GameWatch.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.GameWatch.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.GameWatch.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.GameWatch.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.GameWatch.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.GameWatch.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.GameWatch.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.GameWatch.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.GameWatch.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.GameWatch.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.GameWatch.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.GameWatch.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.GameWatch.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.GameWatch.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.GameWatch.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.GameWatch.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.GameWatch.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.GameWatch.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.GameWatch.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.GameWatch.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.GameWatch.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.GameWatch.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.GameWatch.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.GameWatch.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.GameWatch.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.GameWatch.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.GameWatch.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.GameWatch.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.GameWatch.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.GameWatch.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.GameWatch.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.GameWatch.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.GameWatch.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.GameWatch.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.GameWatch.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.GameWatch.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.GameWatch.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.GameWatch.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.GameWatch.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.GameWatch.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.GameWatch.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}



