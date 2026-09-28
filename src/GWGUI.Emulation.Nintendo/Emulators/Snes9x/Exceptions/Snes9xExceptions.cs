using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Snes9x.Exceptions;

internal static class Snes9xExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Snes9x.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Snes9x.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Snes9x.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Snes9x.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Snes9x.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Snes9x.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Snes9x.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Snes9x.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Snes9x.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Snes9x.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Snes9x.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Snes9x.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Snes9x.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Snes9x.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Snes9x.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Snes9x.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Snes9x.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Snes9x.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Snes9x.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Snes9x.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Snes9x.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Snes9x.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Snes9x.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Snes9x.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Snes9x.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Snes9x.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Snes9x.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Snes9x.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Snes9x.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Snes9x.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Snes9x.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Snes9x.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Snes9x.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Snes9x.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Snes9x.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Snes9x.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Snes9x.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Snes9x.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Snes9x.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Snes9x.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Snes9x.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Snes9x.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Snes9x.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Snes9x.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Snes9x.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Snes9x.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


