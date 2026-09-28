using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.BeetleVb.Exceptions;

internal static class BeetleVbExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.BeetleVb.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.BeetleVb.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.BeetleVb.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.BeetleVb.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.BeetleVb.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.BeetleVb.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.BeetleVb.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.BeetleVb.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.BeetleVb.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.BeetleVb.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.BeetleVb.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.BeetleVb.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.BeetleVb.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.BeetleVb.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.BeetleVb.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.BeetleVb.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.BeetleVb.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.BeetleVb.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.BeetleVb.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.BeetleVb.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.BeetleVb.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.BeetleVb.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.BeetleVb.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.BeetleVb.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.BeetleVb.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.BeetleVb.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.BeetleVb.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.BeetleVb.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.BeetleVb.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.BeetleVb.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.BeetleVb.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.BeetleVb.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.BeetleVb.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.BeetleVb.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.BeetleVb.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.BeetleVb.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.BeetleVb.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.BeetleVb.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.BeetleVb.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.BeetleVb.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.BeetleVb.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.BeetleVb.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.BeetleVb.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.BeetleVb.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.BeetleVb.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.BeetleVb.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
