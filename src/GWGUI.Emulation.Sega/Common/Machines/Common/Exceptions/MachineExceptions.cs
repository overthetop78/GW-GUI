using System.Globalization;
using GWGUI.Emulation.Sega.Modules;

namespace GWGUI.Emulation.Sega.Common.Machines.Common.Exceptions;

internal static class MachineExceptions
{
    internal static EmulationMessageException SavedStateInvalid(Exception? innerException = null) =>
        Error(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateInvalid, innerException);

    internal static EmulationMessageException SavedStateIncompatible(Exception? innerException = null) =>
        Error(EmulationMessageCategory.SavedState, EmulationMessageCode.SavedStateIncompatible,
            innerException);

    internal static EmulationMessageException MediaNotFound(Exception? innerException = null) =>
        Error(EmulationMessageCategory.Media, EmulationMessageCode.MediaNotFound, innerException);

    internal static EmulationMessageException MachineNotRunning(Exception? innerException = null) =>
        Error(EmulationMessageCategory.Machine, EmulationMessageCode.MachineStartFailed, innerException);

    private static EmulationMessageException Error(EmulationMessageCategory category,
        EmulationMessageCode code, Exception? innerException) =>
        new(new EmulationMessage(category, code, EmulationMessageSeverity.Error,
            EmulationMessageTarget.Dialog), innerException);
}

internal static class ExternalCoreExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(SegaEmulationModule).Assembly, "GWGUI.Emulation.Sega.Resources.Error");

    internal static string HostConfigurationInvalid() => Text("Error.ExternalCore.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Error.ExternalCore.HostNotInitialized");
    internal static string CoreNotInitialized() => Text("Error.ExternalCore.CoreNotInitialized");
    internal static string MediaNotFound() => Text("Error.ExternalCore.MediaNotFound");
    internal static string FullContentPathsRequired() => Text("Error.ExternalCore.FullContentPathsRequired");
    internal static string StartWithoutMediaUnsupported() => Text("Error.ExternalCore.StartWithoutMediaUnsupported");
    internal static string ContentRefused() => Text("Error.ExternalCore.ContentRefused");
    internal static string PlaylistLimitExceeded() => Text("Error.ExternalCore.PlaylistLimitExceeded");
    internal static string DiskLabelInvalid() => Text("Error.ExternalCore.DiskLabelInvalid");
    internal static string StateSaveFailed() => Text("Error.ExternalCore.StateSaveFailed");
    internal static string StateEmpty() => Text("Error.ExternalCore.StateEmpty");
    internal static string StateRestoreFailed() => Text("Error.ExternalCore.StateRestoreFailed");
    internal static string CoreNotLoaded() => Text("Error.ExternalCore.CoreNotLoaded");
    internal static string CorePathNotAbsolute() => Text("Error.ExternalCore.CorePathNotAbsolute");
    internal static string CoreNotFound() => Text("Error.ExternalCore.CoreNotFound");
    internal static string ArchiveMissingLibrary() => Text("Error.ExternalCore.ArchiveMissingLibrary");
    internal static string DownloadedCoreNotPe() => Text("Error.ExternalCore.DownloadedCoreNotPe");
    internal static string DownloadedCoreInvalidPe() => Text("Error.ExternalCore.DownloadedCoreInvalidPe");
    internal static string DownloadedCoreWrongArchitecture() => Text("Error.ExternalCore.DownloadedCoreWrongArchitecture");
    internal static string MediaEjectFailed() => Text("Error.ExternalCore.MediaEjectFailed");
    internal static string RequestedDiskSelectionFailed() => Text("Error.ExternalCore.RequestedDiskSelectionFailed");
    internal static string RequestedMediaInsertFailed() => Text("Error.ExternalCore.RequestedMediaInsertFailed");
    internal static string MediaSlotCreationFailed() => Text("Error.ExternalCore.MediaSlotCreationFailed");
    internal static string MediaRefused() => Text("Error.ExternalCore.MediaRefused");
    internal static string MediaSelectionFailed() => Text("Error.ExternalCore.MediaSelectionFailed");
    internal static string MediaInsertFailed() => Text("Error.ExternalCore.MediaInsertFailed");
    internal static string DiskControlUnavailable() => Text("Error.ExternalCore.DiskControlUnavailable");
    internal static string DiskControlIncomplete() => Text("Error.ExternalCore.DiskControlIncomplete");
    internal static string ProcessAlreadyInitialized() => Text("Error.ExternalCore.ProcessAlreadyInitialized");
    internal static string HostExecutableNotFound() => Text("Error.ExternalCore.HostExecutableNotFound");
    internal static string ProcessStartFailed() => Text("Error.ExternalCore.ProcessStartFailed");
    internal static string VideoBufferUnavailable() => Text("Error.ExternalCore.VideoBufferUnavailable");
    internal static string ProcessUnavailable() => Text("Error.ExternalCore.ProcessUnavailable");
    internal static string ProcessNotInitialized() => Text("Error.ExternalCore.ProcessNotInitialized");
    internal static string ProcessTimeout() => Text("Error.ExternalCore.ProcessTimeout");
    internal static string ProcessCommunicationFailed(string detail) => Text("Error.ExternalCore.ProcessCommunicationFailed", detail);
    internal static string HostResponseUnavailable() => Text("Error.ExternalCore.HostResponseUnavailable");
    internal static string UnknownCoreOption() => Text("Error.ExternalCore.UnknownCoreOption");
    internal static string UnsupportedPixelFormat(int value) => Text("Error.ExternalCore.UnsupportedPixelFormat", value);
    internal static string InvalidOptionValue(string value, string key) => Text("Error.ExternalCore.InvalidOptionValue", value, key);
    internal static string InvalidResponseLength(int length) => Text("Error.ExternalCore.InvalidResponseLength", length);
    internal static string UnsupportedApiVersion(uint version) => Text("Error.ExternalCore.UnsupportedApiVersion", version);
    internal static string LibraryIdentityMismatch(string? name) => Text("Error.ExternalCore.LibraryIdentityMismatch", name ?? string.Empty);
    internal static string UnsupportedContentExtension(string extension) => Text("Error.ExternalCore.UnsupportedContentExtension", extension);
    internal static string InvalidStateSize(nuint size) => Text("Error.ExternalCore.InvalidStateSize", size);
    internal static string UnknownHostCommand(byte command) => Text("Error.ExternalCore.UnknownHostCommand", command);

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
