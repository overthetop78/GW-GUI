using System.Globalization;
using GWGUI.Emulation.Nec.Modules;

namespace GWGUI.Emulation.Nec.Emulators.BeetlePcfx.Exceptions;

internal static class BeetlePcfxExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(NecEmulationModule).Assembly, "GWGUI.Emulation.Nec.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.BeetlePcfx.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.BeetlePcfx.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Emulation.Error.BeetlePcfx.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Emulation.Error.BeetlePcfx.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Emulation.Error.BeetlePcfx.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Emulation.Error.BeetlePcfx.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Emulation.Error.BeetlePcfx.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Emulation.Error.BeetlePcfx.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Emulation.Error.BeetlePcfx.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Emulation.Error.BeetlePcfx.StateSaveFailed");
    internal static string StateEmpty() => Text("Emulation.Error.BeetlePcfx.StateEmpty");
    internal static string StateRestoreFailed() => Text("Emulation.Error.BeetlePcfx.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Emulation.Error.BeetlePcfx.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Emulation.Error.BeetlePcfx.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Emulation.Error.BeetlePcfx.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Emulation.Error.BeetlePcfx.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Emulation.Error.BeetlePcfx.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Emulation.Error.BeetlePcfx.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Emulation.Error.BeetlePcfx.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Emulation.Error.BeetlePcfx.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Emulation.Error.BeetlePcfx.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Emulation.Error.BeetlePcfx.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Emulation.Error.BeetlePcfx.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Emulation.Error.BeetlePcfx.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Emulation.Error.BeetlePcfx.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Emulation.Error.BeetlePcfx.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Emulation.Error.BeetlePcfx.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Emulation.Error.BeetlePcfx.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Emulation.Error.BeetlePcfx.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Emulation.Error.BeetlePcfx.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Emulation.Error.BeetlePcfx.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Emulation.Error.BeetlePcfx.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Emulation.Error.BeetlePcfx.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Emulation.Error.BeetlePcfx.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Emulation.Error.BeetlePcfx.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) =>
        Text("Emulation.Error.BeetlePcfx.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Emulation.Error.BeetlePcfx.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Emulation.Error.BeetlePcfx.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Emulation.Error.BeetlePcfx.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Emulation.Error.BeetlePcfx.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Emulation.Error.BeetlePcfx.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Emulation.Error.BeetlePcfx.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Emulation.Error.BeetlePcfx.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Emulation.Error.BeetlePcfx.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Emulation.Error.BeetlePcfx.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.BeetlePcfx.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}

