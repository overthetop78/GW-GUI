using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Mupen64PlusNext.Exceptions;

internal static class Mupen64PlusNextExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Mupen64PlusNext.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Mupen64PlusNext.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Mupen64PlusNext.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Mupen64PlusNext.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Mupen64PlusNext.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Mupen64PlusNext.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Mupen64PlusNext.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Mupen64PlusNext.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Mupen64PlusNext.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Mupen64PlusNext.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Mupen64PlusNext.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Mupen64PlusNext.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Mupen64PlusNext.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Mupen64PlusNext.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Mupen64PlusNext.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Mupen64PlusNext.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Mupen64PlusNext.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Mupen64PlusNext.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Mupen64PlusNext.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Mupen64PlusNext.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Mupen64PlusNext.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Mupen64PlusNext.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Mupen64PlusNext.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Mupen64PlusNext.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Mupen64PlusNext.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Mupen64PlusNext.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Mupen64PlusNext.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Mupen64PlusNext.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Mupen64PlusNext.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Mupen64PlusNext.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Mupen64PlusNext.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Mupen64PlusNext.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Mupen64PlusNext.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Mupen64PlusNext.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Mupen64PlusNext.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Mupen64PlusNext.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Mupen64PlusNext.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Mupen64PlusNext.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Mupen64PlusNext.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Mupen64PlusNext.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Mupen64PlusNext.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Mupen64PlusNext.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Mupen64PlusNext.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Mupen64PlusNext.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Mupen64PlusNext.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Mupen64PlusNext.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


