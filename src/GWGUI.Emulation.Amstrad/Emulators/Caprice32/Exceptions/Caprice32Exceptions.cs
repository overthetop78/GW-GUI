using System.Globalization;
using GWGUI.Emulation.Amstrad.Modules;

namespace GWGUI.Emulation.Amstrad.Emulators.Caprice32.Exceptions;

internal static class Caprice32Exceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AmstradEmulationModule).Assembly, "GWGUI.Emulation.Amstrad.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Caprice32.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Caprice32.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Caprice32.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Caprice32.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Caprice32.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Caprice32.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Caprice32.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Caprice32.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Caprice32.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Caprice32.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Caprice32.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Caprice32.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Caprice32.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Caprice32.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Caprice32.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Caprice32.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Caprice32.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Caprice32.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Caprice32.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Caprice32.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Caprice32.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Caprice32.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Caprice32.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Caprice32.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Caprice32.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Caprice32.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Caprice32.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Caprice32.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Caprice32.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Caprice32.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Caprice32.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Caprice32.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Caprice32.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Caprice32.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Caprice32.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Caprice32.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Caprice32.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Caprice32.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Caprice32.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Caprice32.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Caprice32.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Caprice32.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Caprice32.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Caprice32.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Caprice32.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Caprice32.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

