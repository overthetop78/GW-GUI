namespace GWGUI.Emulation.Apple.Common.Exceptions;

internal static class ErrorMessages
{
    internal static string InvalidArchitecture => ExceptionText.Get(CoreErrorResourceKeys.CoreRejected);
    internal static string MissingPublishedDate => ExceptionText.Get(CoreErrorResourceKeys.CoreRejected);
    internal static string MissingExpectedLibraryFormat => ExceptionText.Get(CoreErrorResourceKeys.CoreRejected);
    internal static string InstalledLibraryLockedFormat => ExceptionText.Get(CoreErrorResourceKeys.CoreRejected);
    internal static string EmptyInstallationRoot => ExceptionText.Get(CoreErrorResourceKeys.CoreNotFound);
    internal static string EmptyVersion => ExceptionText.Get(CoreErrorResourceKeys.CoreNotFound);
    internal static string DuplicateCore => ExceptionText.Get(CoreErrorResourceKeys.CoreRejected);
    internal static string MissingModel => ExceptionText.Get(CoreErrorResourceKeys.CoreNotFound);


    internal static string UnknownFirmware => FirmwareInvalid();
    internal static string RequiredFirmwareMissing => FirmwareMissing();
    internal static string FirmwareFileMissing => FirmwareMissing();
    internal static string FirmwareFileUnreadable => FirmwareInvalid();
    internal static string FirmwareIdentityAmbiguous => FirmwareInvalid();
    internal static string FirmwareCannotBeSelected => FirmwareInvalid();
    internal static string DuplicateHardwareModelDefinition => OptionInvalid();
    internal static string UnknownHardwareModel => OptionInvalid();
    internal static string DuplicateCompatibilityDefinition => OptionInvalid();
    internal static string UnknownCompatibilityModel => OptionInvalid();
    internal static string MissingUnavailableExplanation => OptionInvalid();
    internal static string InvalidCompatibilityControllerCount => OptionInvalid();
    internal static string UnsupportedSchema => OptionInvalid();
    internal static string EmptyFirmwarePath => FirmwareMissing();
    internal static string DuplicateFirmware => FirmwareInvalid();
    internal static string IncompatibleFirmware => FirmwareInvalid();
    internal static string EmptyMediaPath => ContentNotFound();
    internal static string DuplicateMediaSlot => ContentUnsupported();
    internal static string IncompatibleMedia => ContentUnsupported();
    internal static string InvalidControllerPort => OptionInvalid();
    internal static string UnsupportedControllerDevice => OptionInvalid();
    internal static string DuplicateControllerPort => OptionInvalid();
    internal static string InvalidControllerDeadZone => OptionInvalid();
    internal static string CorePathMustBeAbsolute => CoreNotFound();
    internal static string CoreFileMissing => CoreNotFound();
    internal static string CoreExportMissing => CoreRejected();
    internal static string CoreApiVersionUnsupported => CoreRejected();
    internal static string CoreIdentityMismatch => CoreRejected();
    internal static string ContentFileMissing => ContentNotFound();
    internal static string ContentExtensionUnsupported => ContentUnsupported();
    internal static string OptionValueInvalidFormat => OptionInvalid();
    internal static string CoreAlreadyInitialized => CoreRejected();
    internal static string CoreNotInitialized => CoreRejected();
    internal static string ContentLoadFailed => ContentUnsupported();
    internal static string ContentRequired => ContentUnsupported();
    internal static string DynamicMediaUnsupported => ContentUnsupported();
    internal static string StorageExtensionInvalid => ContentUnsupported();
    internal static string StateInvalid => StateInvalidText();
    internal static string StateSizeInvalid => StateInvalidText();
    internal static string StateUnavailable => StateInvalidText();
    internal static string StateSaveFailed => StateInvalidText();
    internal static string StateLoadFailed => StateInvalidText();
    internal static string StateIncompatible => ExceptionText.Get(CoreErrorResourceKeys.StateIncompatible);
    internal static string MachineInvalidState => ExceptionText.Get(CoreErrorResourceKeys.Unexpected);
    internal static string MachineStopped => ExceptionText.Get(CoreErrorResourceKeys.Unexpected);

    private static string FirmwareMissing() => ExceptionText.Get(CoreErrorResourceKeys.FirmwareMissing);
    private static string FirmwareInvalid() => ExceptionText.Get(CoreErrorResourceKeys.FirmwareInvalid);
    private static string ContentNotFound() => ExceptionText.Get(CoreErrorResourceKeys.ContentNotFound);
    private static string ContentUnsupported() => ExceptionText.Get(CoreErrorResourceKeys.ContentUnsupported);
    private static string OptionInvalid() => ExceptionText.Get(CoreErrorResourceKeys.OptionInvalid);
    private static string CoreNotFound() => ExceptionText.Get(CoreErrorResourceKeys.CoreNotFound);
    private static string CoreRejected() => ExceptionText.Get(CoreErrorResourceKeys.CoreRejected);
    private static string StateInvalidText() => ExceptionText.Get(CoreErrorResourceKeys.StateInvalid);

}
