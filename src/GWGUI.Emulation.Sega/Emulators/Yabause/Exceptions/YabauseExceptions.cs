using System.Globalization;
using GWGUI.Emulation.Sega.Modules;

namespace GWGUI.Emulation.Sega.Emulators.Yabause.Exceptions;

internal static class YabauseExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SegaEmulationModule).Assembly, "GWGUI.Emulation.Sega.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Yabause.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Yabause.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Yabause.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Yabause.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Yabause.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Yabause.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Yabause.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Yabause.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Yabause.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Yabause.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Yabause.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Yabause.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Yabause.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Yabause.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Yabause.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Yabause.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Yabause.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Yabause.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Yabause.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Yabause.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Yabause.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Yabause.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Yabause.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Yabause.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Yabause.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Yabause.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Yabause.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Yabause.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Yabause.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Yabause.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Yabause.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Yabause.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Yabause.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Yabause.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Yabause.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Yabause.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Yabause.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Yabause.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Yabause.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Yabause.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Yabause.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Yabause.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Yabause.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Yabause.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Yabause.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Yabause.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

