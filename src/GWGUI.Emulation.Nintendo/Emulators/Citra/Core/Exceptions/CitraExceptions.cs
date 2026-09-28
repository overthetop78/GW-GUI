using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Citra.Exceptions;

internal static class CitraExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Citra.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Citra.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Citra.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Citra.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Citra.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Citra.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Citra.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Citra.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Citra.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Citra.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Citra.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Citra.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Citra.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Citra.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Citra.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Citra.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Citra.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Citra.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Citra.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Citra.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Citra.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Citra.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Citra.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Citra.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Citra.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Citra.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Citra.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Citra.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Citra.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Citra.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Citra.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Citra.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Citra.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Citra.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Citra.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Citra.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Citra.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Citra.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Citra.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Citra.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Citra.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Citra.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Citra.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Citra.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Citra.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Citra.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


