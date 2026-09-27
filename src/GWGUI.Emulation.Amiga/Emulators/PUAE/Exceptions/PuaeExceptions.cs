using System.Globalization;
using GWGUI.Emulation.Amiga.Modules;

namespace GWGUI.Emulation.Amiga.Emulators.PUAE.Exceptions;

internal static class PuaeExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AmigaEmulationModule).Assembly, "GWGUI.Emulation.Amiga.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.PUAE.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.PUAE.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.PUAE.CoreNotInitialized");
    internal static string KickstartNotFound() => Text("Emulation.Error.PUAE.KickstartNotFound");
    internal static string MediaNotFound() => Text("Emulation.Error.PUAE.MediaNotFound");
    internal static string ExtendedRomNotFound() => Text("Emulation.Error.PUAE.ExtendedRomNotFound");
    internal static string RomKeyNotFound() => Text("Emulation.Error.PUAE.RomKeyNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.PUAE.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.PUAE.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.PUAE.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.PUAE.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.PUAE.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.PUAE.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.PUAE.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.PUAE.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.PUAE.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.PUAE.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.PUAE.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.PUAE.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.PUAE.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.PUAE.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.PUAE.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.PUAE.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.PUAE.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.PUAE.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.PUAE.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.PUAE.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.PUAE.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.PUAE.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.PUAE.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.PUAE.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.PUAE.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.PUAE.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.PUAE.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.PUAE.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.PUAE.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.PUAE.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.PUAE.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.PUAE.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.PUAE.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.PUAE.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.PUAE.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.PUAE.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.PUAE.InvalidResponseLength", length);
    internal static string UnsupportedHardDiskExtension() => Text("Emulation.Error.PUAE.UnsupportedHardDiskExtension");
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.PUAE.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.PUAE.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.PUAE.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.PUAE.InvalidStateSize", size);
    internal static string UnsupportedController(string name, int port) => Text("Emulation.Error.PUAE.UnsupportedController", name, port);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.PUAE.UnknownHostCommand", command);
    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0
            ? value
            : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
