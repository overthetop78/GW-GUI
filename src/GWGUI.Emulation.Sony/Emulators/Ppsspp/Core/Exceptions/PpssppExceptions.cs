using System.Globalization;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Emulation.Sony.Emulators.Ppsspp.Exceptions;

internal static class PpssppExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SonyEmulationModule).Assembly, "GWGUI.Emulation.Sony.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Ppsspp.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Ppsspp.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Ppsspp.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Ppsspp.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Ppsspp.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Ppsspp.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Ppsspp.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Ppsspp.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Ppsspp.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Ppsspp.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Ppsspp.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Ppsspp.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Ppsspp.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Ppsspp.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Ppsspp.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Ppsspp.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Ppsspp.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Ppsspp.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Ppsspp.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Ppsspp.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Ppsspp.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Ppsspp.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Ppsspp.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Ppsspp.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Ppsspp.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Ppsspp.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Ppsspp.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Ppsspp.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Ppsspp.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Ppsspp.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Ppsspp.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Ppsspp.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Ppsspp.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Ppsspp.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Ppsspp.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Ppsspp.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Ppsspp.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Ppsspp.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Ppsspp.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Ppsspp.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Ppsspp.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Ppsspp.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Ppsspp.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Ppsspp.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Ppsspp.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Ppsspp.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

