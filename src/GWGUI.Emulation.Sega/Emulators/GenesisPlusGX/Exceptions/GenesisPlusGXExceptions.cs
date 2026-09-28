using System.Globalization;
using GWGUI.Emulation.Sega.Modules;

namespace GWGUI.Emulation.Sega.Emulators.GenesisPlusGX.Exceptions;

internal static class GenesisPlusGXExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SegaEmulationModule).Assembly, "GWGUI.Emulation.Sega.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.GenesisPlusGX.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.GenesisPlusGX.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.GenesisPlusGX.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.GenesisPlusGX.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.GenesisPlusGX.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.GenesisPlusGX.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.GenesisPlusGX.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.GenesisPlusGX.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.GenesisPlusGX.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.GenesisPlusGX.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.GenesisPlusGX.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.GenesisPlusGX.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.GenesisPlusGX.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.GenesisPlusGX.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.GenesisPlusGX.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.GenesisPlusGX.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.GenesisPlusGX.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.GenesisPlusGX.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.GenesisPlusGX.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.GenesisPlusGX.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.GenesisPlusGX.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.GenesisPlusGX.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.GenesisPlusGX.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.GenesisPlusGX.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.GenesisPlusGX.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.GenesisPlusGX.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.GenesisPlusGX.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.GenesisPlusGX.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.GenesisPlusGX.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.GenesisPlusGX.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.GenesisPlusGX.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.GenesisPlusGX.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.GenesisPlusGX.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.GenesisPlusGX.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.GenesisPlusGX.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.GenesisPlusGX.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.GenesisPlusGX.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.GenesisPlusGX.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.GenesisPlusGX.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.GenesisPlusGX.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.GenesisPlusGX.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.GenesisPlusGX.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.GenesisPlusGX.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.GenesisPlusGX.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.GenesisPlusGX.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.GenesisPlusGX.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

