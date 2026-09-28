using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Mgba.Exceptions;

internal static class MgbaExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Mgba.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Mgba.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Mgba.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Mgba.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Mgba.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Mgba.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Mgba.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Mgba.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Mgba.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Mgba.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Mgba.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Mgba.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Mgba.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Mgba.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Mgba.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Mgba.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Mgba.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Mgba.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Mgba.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Mgba.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Mgba.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Mgba.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Mgba.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Mgba.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Mgba.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Mgba.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Mgba.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Mgba.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Mgba.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Mgba.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Mgba.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Mgba.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Mgba.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Mgba.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Mgba.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Mgba.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Mgba.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Mgba.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Mgba.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Mgba.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Mgba.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Mgba.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Mgba.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Mgba.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Mgba.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Mgba.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


