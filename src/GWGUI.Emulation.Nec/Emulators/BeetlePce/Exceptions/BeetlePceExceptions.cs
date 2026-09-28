using System.Globalization;
using GWGUI.Emulation.Nec.Modules;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePce.Exceptions;

internal static class BeetlePceExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NecEmulationModule).Assembly, "GWGUI.Emulation.Nec.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.BeetlePce.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.BeetlePce.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.BeetlePce.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.BeetlePce.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.BeetlePce.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.BeetlePce.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.BeetlePce.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.BeetlePce.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.BeetlePce.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.BeetlePce.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.BeetlePce.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.BeetlePce.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.BeetlePce.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.BeetlePce.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.BeetlePce.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.BeetlePce.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.BeetlePce.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.BeetlePce.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.BeetlePce.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.BeetlePce.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.BeetlePce.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.BeetlePce.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.BeetlePce.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.BeetlePce.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.BeetlePce.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.BeetlePce.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.BeetlePce.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.BeetlePce.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.BeetlePce.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.BeetlePce.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.BeetlePce.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.BeetlePce.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.BeetlePce.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.BeetlePce.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.BeetlePce.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.BeetlePce.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.BeetlePce.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.BeetlePce.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.BeetlePce.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.BeetlePce.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.BeetlePce.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.BeetlePce.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.BeetlePce.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.BeetlePce.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.BeetlePce.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.BeetlePce.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

