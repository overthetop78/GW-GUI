using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Gambatte.Exceptions;

internal static class GambatteExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Gambatte.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Gambatte.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Gambatte.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Gambatte.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Gambatte.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Gambatte.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Gambatte.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Gambatte.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Gambatte.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Gambatte.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Gambatte.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Gambatte.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Gambatte.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Gambatte.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Gambatte.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Gambatte.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Gambatte.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Gambatte.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Gambatte.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Gambatte.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Gambatte.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Gambatte.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Gambatte.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Gambatte.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Gambatte.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Gambatte.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Gambatte.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Gambatte.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Gambatte.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Gambatte.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Gambatte.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Gambatte.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Gambatte.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Gambatte.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Gambatte.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Gambatte.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Gambatte.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Gambatte.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Gambatte.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Gambatte.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Gambatte.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Gambatte.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Gambatte.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Gambatte.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Gambatte.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Gambatte.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

