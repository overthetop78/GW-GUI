namespace GWGUI.Emulation.Watara.Common.Exceptions;

internal static class ErrorMessages
{
    internal static string InvalidArchitecture => ExceptionText.Get(CoreErrorResourceKeys.InvalidArchitecture);
    internal static string MissingPublishedDate => ExceptionText.Get(CoreErrorResourceKeys.MissingPublishedDate);
    internal static string MissingExpectedLibraryFormat => ExceptionText.Get(CoreErrorResourceKeys.MissingExpectedLibraryFormat);
    internal static string InstalledLibraryLockedFormat => ExceptionText.Get(CoreErrorResourceKeys.InstalledLibraryLockedFormat);

    internal static string MachineInvalidState => ExceptionText.Get(CoreErrorResourceKeys.MachineInvalidState);
    internal static string MachineStopped => ExceptionText.Get(CoreErrorResourceKeys.MachineStopped);
    internal static string UnsupportedControllerDevice => ExceptionText.Get(CoreErrorResourceKeys.UnsupportedControllerDevice);
    internal static string DynamicMediaUnsupported => ExceptionText.Get(CoreErrorResourceKeys.DynamicMediaUnsupported);
    internal static string StateInvalid => ExceptionText.Get(CoreErrorResourceKeys.StateInvalid);
    internal static string StateUnavailable => ExceptionText.Get(CoreErrorResourceKeys.StateUnavailable);
    internal static string StateIncompatible => ExceptionText.Get(CoreErrorResourceKeys.StateIncompatible);
    internal static string ContentFileMissing => ExceptionText.Get(CoreErrorResourceKeys.ContentFileMissing);
}
