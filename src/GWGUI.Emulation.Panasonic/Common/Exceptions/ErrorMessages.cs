namespace GWGUI.Emulation.Panasonic.Common.Exceptions;

internal static class ErrorMessages
{
    internal const string InvalidArchitecture = "The downloaded core is not Windows x64.";
    internal const string MissingPublishedDate = "The official response does not identify its publication date.";
    internal const string MissingExpectedLibraryFormat = "The downloaded archive does not contain {0}.";
    internal const string InstalledLibraryLockedFormat = "The installed library cannot be replaced: {0}.";

    internal const string MachineInvalidState = "The emulation session is not in the required state.";
    internal const string MachineStopped = "The emulation session has stopped.";
    internal const string UnsupportedControllerDevice = "The controller device is not supported.";
    internal const string DynamicMediaUnsupported = "The emulation session cannot exchange this medium.";
    internal const string StateInvalid = "The saved state is invalid.";
    internal const string StateUnavailable = "The emulator does not support saved states.";
    internal const string StateIncompatible = "The saved state is incompatible with the current machine.";
    internal const string ContentFileMissing = "The configured medium could not be found.";
}
