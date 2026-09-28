using System.Globalization;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Emulation.Sony.Emulators.Pcsx2.Exceptions;

internal static class Pcsx2Exceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SonyEmulationModule).Assembly, "GWGUI.Emulation.Sony.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Pcsx2.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Pcsx2.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Pcsx2.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Pcsx2.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Pcsx2.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Pcsx2.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Pcsx2.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Pcsx2.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Pcsx2.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Pcsx2.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Pcsx2.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Pcsx2.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Pcsx2.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Pcsx2.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Pcsx2.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Pcsx2.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Pcsx2.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Pcsx2.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Pcsx2.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Pcsx2.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Pcsx2.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Pcsx2.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Pcsx2.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Pcsx2.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Pcsx2.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Pcsx2.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Pcsx2.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Pcsx2.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Pcsx2.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Pcsx2.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Pcsx2.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Pcsx2.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Pcsx2.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Pcsx2.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Pcsx2.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Pcsx2.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Pcsx2.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Pcsx2.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Pcsx2.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Pcsx2.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Pcsx2.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Pcsx2.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Pcsx2.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Pcsx2.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Pcsx2.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Pcsx2.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

