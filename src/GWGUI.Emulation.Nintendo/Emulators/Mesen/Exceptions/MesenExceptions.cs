using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Mesen.Exceptions;

internal static class MesenExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Mesen.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Mesen.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Mesen.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Mesen.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Mesen.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Mesen.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Mesen.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Mesen.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Mesen.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Mesen.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Mesen.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Mesen.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Mesen.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Mesen.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Mesen.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Mesen.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Mesen.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Mesen.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Mesen.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Mesen.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Mesen.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Mesen.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Mesen.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Mesen.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Mesen.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Mesen.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Mesen.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Mesen.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Mesen.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Mesen.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Mesen.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Mesen.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Mesen.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Mesen.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Mesen.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Mesen.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Mesen.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Mesen.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Mesen.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Mesen.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Mesen.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Mesen.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Mesen.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Mesen.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Mesen.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Mesen.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


