using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.MelonDs.Exceptions;

internal static class MelonDsExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.MelonDs.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.MelonDs.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.MelonDs.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.MelonDs.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.MelonDs.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.MelonDs.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.MelonDs.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.MelonDs.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.MelonDs.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.MelonDs.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.MelonDs.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.MelonDs.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.MelonDs.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.MelonDs.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.MelonDs.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.MelonDs.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.MelonDs.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.MelonDs.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.MelonDs.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.MelonDs.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.MelonDs.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.MelonDs.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.MelonDs.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.MelonDs.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.MelonDs.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.MelonDs.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.MelonDs.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.MelonDs.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.MelonDs.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.MelonDs.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.MelonDs.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.MelonDs.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.MelonDs.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.MelonDs.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.MelonDs.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.MelonDs.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.MelonDs.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.MelonDs.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.MelonDs.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.MelonDs.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.MelonDs.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.MelonDs.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.MelonDs.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.MelonDs.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.MelonDs.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.MelonDs.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


