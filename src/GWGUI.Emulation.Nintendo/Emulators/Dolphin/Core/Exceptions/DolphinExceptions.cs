using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Dolphin.Exceptions;

internal static class DolphinExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Dolphin.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Dolphin.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Dolphin.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Dolphin.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Dolphin.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Dolphin.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Dolphin.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Dolphin.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Dolphin.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Dolphin.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Dolphin.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Dolphin.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Dolphin.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Dolphin.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Dolphin.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Dolphin.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Dolphin.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Dolphin.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Dolphin.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Dolphin.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Dolphin.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Dolphin.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Dolphin.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Dolphin.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Dolphin.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Dolphin.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Dolphin.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Dolphin.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Dolphin.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Dolphin.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Dolphin.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Dolphin.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Dolphin.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Dolphin.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Dolphin.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Dolphin.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Dolphin.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Dolphin.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Dolphin.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Dolphin.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Dolphin.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Dolphin.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Dolphin.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Dolphin.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Dolphin.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Dolphin.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


