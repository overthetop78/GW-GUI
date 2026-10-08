using System.Globalization;
using GWGUI.Emulation.Nintendo.Modules;

namespace GWGUI.Emulation.Nintendo.Emulators.Common.Interop.Exceptions;

internal static class CoreExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NintendoEmulationModule).Assembly, "GWGUI.Emulation.Nintendo.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Core.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Core.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.Core.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.Core.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.Core.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.Core.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.Core.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.Core.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.Core.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.Core.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.Core.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.Core.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.Core.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.Core.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.Core.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.Core.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.Core.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.Core.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.Core.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.Core.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.Core.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.Core.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.Core.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.Core.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.Core.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.Core.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.Core.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.Core.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.Core.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.Core.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.Core.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.Core.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.Core.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.Core.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.Core.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.Core.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.Core.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.Core.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.Core.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.Core.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.Core.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.Core.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.Core.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.Core.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.Core.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Core.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}


